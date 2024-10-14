import threading
import queue
import math
import numpy as np
import cv2
import pyautogui
from pynput import keyboard
from ultralytics import YOLO
from ultralytics.engine.results import Results
from BodyInfo import BodyInfo

STRIDE = 32
SCALE = 10
IMG_H = STRIDE * int(SCALE / 4 * 3)
IMG_W = STRIDE * SCALE

CAM_DEVICE_ID = 0 # 1: outer webcam, 0: stock camera

MOUSE_MOVE_COEF_H = 1.2
MOUSE_MOVE_COEF_V = 0.5

MOVE_AREA_LIMIT = 0.95 # ウィンドウ内でのマウスの稼働領域を制限
WINDOW_HEIGHT = 1080
WINDOW_WIDTH = 1920

# control flags
stop_flag = False # キー入力を追跡するためのフラグ
mouse_move_flag = True  # マウス移動のオン/オフを管理するフラグ
imshow_flag = False # 推論結果の画像を表示するか否かを管理するフラグ

# フレームキューの作成
frame_queue = queue.Queue(maxsize=2)  # キューの最大サイズを設定してメモリを節約


def async_camera_capture():
    '''別スレッドでカメラからフレームを取得してキューに追加する'''
    global cam, stop_flag
    while not stop_flag:
        ret, img = cam.read()
        if ret and img is not None:
            img = cv2.resize(img, (IMG_W, IMG_H))
            img = cv2.flip(img, 1)  # 画像を左右反転
            if not frame_queue.full():
                frame_queue.put(img)

def main(bodyinfo: BodyInfo):
    global stop_flag, mouse_move_flag, imshow_flag
    if frame_queue.empty():
        return 0  # フレームがなければ何もしない

    img = frame_queue.get()
    result = model.predict(
        img,
        imgsz=(IMG_H, IMG_W),
        device='cpu',
        verbose=False,
    )[0]
    bodyinfo.update(result)

    if bodyinfo.body_tilt is None or math.isnan(bodyinfo.body_tilt):
        tilt = 0
    else:
        tilt = bodyinfo.body_tilt

    if bodyinfo.arm_tilt is None or math.isnan(bodyinfo.arm_tilt):
        angle = 0
    else:
        angle = bodyinfo.arm_tilt

    if np.median(img) < 10: # カメラを塞ぐとマウス操作が可能になる
        mouse_move_flag = False
    # マウス移動が有効な場合のみ動かす
    if mouse_move_flag:
        pyautogui.moveTo(
            (tilt + 1) * MOUSE_MOVE_COEF_H * WINDOW_WIDTH / 2 * MOVE_AREA_LIMIT,
            (-angle + 1) * MOUSE_MOVE_COEF_V * WINDOW_HEIGHT * MOVE_AREA_LIMIT,
            _pause=False,
        )
    if stop_flag:
        return -1
    if imshow_flag:
        plot_img = result.plot(
            kpt_radius=1,
            boxes=False,
        )
        cv2.imshow('camImg', plot_img)
        cv2.waitKey(1)
    cv2.waitKey(1)
    return 0

def on_press(key):
    global stop_flag, mouse_move_flag, imshow_flag
    try:
        if key.char == 'q':
            stop_flag = True
        elif key.char == 'a':
            mouse_move_flag = not mouse_move_flag  # aキーを押すたびにフラグを切り替える
        elif key.char == 'i':
            imshow_flag = not imshow_flag
    except AttributeError:
        pass

print("LOADING MODELS...")
model = YOLO('./yolo11n-pose.pt')
print(f"CONNECTING TO CAMERA {CAM_DEVICE_ID} ...")
cam = cv2.VideoCapture(CAM_DEVICE_ID)


print(f"START INFERENCE ON IMG({IMG_W}, {IMG_H})...")

print("""
    'a': enable/ disable mouse control.
        When you lost window focus or control, hand over on camera and you can get mouse control.
    'q': quit program.
    'i': enable/ disable showing inference result image.
""")
bodyinfo = BodyInfo()

# キー入力を監視
listener = keyboard.Listener(on_press=on_press)
listener.start()

# 非同期カメラキャプチャのスレッド開始
camera_thread = threading.Thread(target=async_camera_capture)
camera_thread.daemon = True
camera_thread.start()

# メインループ
while True:
    flg = main(bodyinfo)
    if flg == -1:
        break

cam.release()
cv2.destroyAllWindows()

print('finish')

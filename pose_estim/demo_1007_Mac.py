import threading
import queue
import math
import numpy as np
import cv2
import pyautogui
from pynput import keyboard
from ultralytics import YOLO
from ultralytics.engine.results import Results

STRIDE = 32
SCALE = 6
IMG_H = STRIDE * int(SCALE / 4 * 3)
IMG_W = STRIDE * SCALE

MOUSE_MOVE_COEF_V = 500
MOUSE_MOVE_COEF_H = 1200

# キー入力を追跡するためのフラグ
stop_flag = False
mouse_move_flag = True  # マウス移動のオン/オフを管理するフラグ

# フレームキューの作成
frame_queue = queue.Queue(maxsize=5)  # キューの最大サイズを設定してメモリを節約

class BodyInfo():
    '''
    Handles keypoints of inference result.
    '''

    def __init__(self, results: Results = None):
        self.keypoints = {
            'eye_r': None,
            'eye_l': None,
            'nose': None,
            'ear_r': None,
            'ear_l': None,
            'shoulder_r': None,
            'shoulder_l': None,
            'elbow_r': None,
            'elbow_l': None,
            'wrist_r': None,
            'wrist_l': None,
            'waist_r': None,
            'waist_l': None,
            'knee_r': None,
            'knee_l': None,
            'ankle_r': None,
            'ankle_l': None,
        }
        self.body_tilt = None
        self.arm_tilt = None
        if results is not None:
            self.update(results)

    def update(self, results: Results):
        if len(results) <= 0:
            return
        results = results[0]
        keypoints = results.keypoints
        self._update_keypoints(keypoints)
        self._update_tilt()
        self._update_angle()

    def _update_tilt(self):
        '''肩の角度で体の傾きを計算する'''
        if self.keypoints['shoulder_r'] == [0.0, 0.0] or self.keypoints['shoulder_l'] == [0.0, 0.0]:
            return
        x_delta = self.keypoints['shoulder_r'][0] - self.keypoints['shoulder_l'][0]
        y_delta = self.keypoints['shoulder_r'][1] - self.keypoints['shoulder_l'][1]
        self.body_tilt = math.sin(math.atan2(y_delta, x_delta))

    def _update_angle(self):
        '''腕の角度を計算する'''

        def get_angle(to_mid, to_elbow):
            norm_mid = np.linalg.norm(to_mid)
            norm_elbow = np.linalg.norm(to_elbow)
            if norm_mid == 0 or norm_elbow == 0:  # ゼロ除算を防ぐ
                return 0  # 角度が不明な場合は0を返す
            cross = np.cross(to_mid, to_elbow)
            sin = cross / (norm_mid * norm_elbow)
            return sin

        shoulder_r = np.array(self.keypoints['shoulder_r'])
        shoulder_l = np.array(self.keypoints['shoulder_l'])
        if shoulder_r.any() == None or shoulder_l.any() == None:
            return
        shoulder_mid = np.array((shoulder_r + shoulder_l) / 2)
        elbow_r = np.array(self.keypoints['elbow_r'])
        elbow_l = np.array(self.keypoints['elbow_l'])
        # r side angle
        sin_r = get_angle(
            shoulder_mid - shoulder_r,
            elbow_r - shoulder_r
        )
        # l side angle
        sin_l = -get_angle(
            shoulder_mid - shoulder_l,
            elbow_l - shoulder_l
        )
        sin = (sin_r + sin_l) / 2
        self.arm_tilt = sin

    def _update_keypoints(self, keypoints):
        xyn = keypoints.xyn[0].tolist()
        self.keypoints['eye_r'] = xyn[1]
        self.keypoints['eye_l'] = xyn[2]
        self.keypoints['nose'] = xyn[0]
        self.keypoints['ear_r'] = xyn[3]
        self.keypoints['ear_l'] = xyn[4]
        self.keypoints['shoulder_r'] = xyn[5]
        self.keypoints['shoulder_l'] = xyn[6]
        self.keypoints['elbow_r'] = xyn[7]
        self.keypoints['elbow_l'] = xyn[8]
        self.keypoints['wrist_r'] = xyn[9]
        self.keypoints['wrist_l'] = xyn[10]
        self.keypoints['waist_r'] = xyn[11]
        self.keypoints['waist_l'] = xyn[12]
        self.keypoints['knee_r'] = xyn[13]
        self.keypoints['knee_l'] = xyn[14]
        self.keypoints['ankle_r'] = xyn[15]
        self.keypoints['ankle_l'] = xyn[16]

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
    global stop_flag, mouse_move_flag
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

    # マウス移動が有効な場合のみ動かす
    if mouse_move_flag:
        pyautogui.moveTo(
            2560 / 3 + tilt *0.9* MOUSE_MOVE_COEF_H,
            1600 / 4 - angle *0.9* MOUSE_MOVE_COEF_V,
            _pause=False,
        )

    if stop_flag:
        return -1
    return 0

def on_press(key):
    global stop_flag, mouse_move_flag
    try:
        if key.char == 'q':
            stop_flag = True
        elif key.char == 'a':
            mouse_move_flag = not mouse_move_flag  # aキーを押すたびにフラグを切り替える
    except AttributeError:
        pass

print("LOADING MODELS...")
model = YOLO('yolo11n-pose.pt')
print("CONNECTING TO CAMERA...")
cam = cv2.VideoCapture(0)


print(f"START INFERENCE ON IMG({IMG_W}, {IMG_H})...")

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

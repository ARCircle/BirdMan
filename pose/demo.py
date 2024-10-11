STRIDE = 32
SCALE = 6
IMG_H = STRIDE * int(SCALE / 4 * 3)
IMG_W = STRIDE * SCALE

MOUSE_MOVE_COEF_H = 1000
MOUSE_MOVE_COEF_V = 200

WINDOW_HEIGHT = 1080
WINDOW_WIDTH = 1920


print("IMPORTING DEPENDENCIES...")
from ultralytics import YOLO
from ultralytics.engine.results import Results

import cv2
import psutil
import os
import math
import mouse
import keyboard
import numpy as np
import pyautogui


class BodyInfo():
    '''
    Handles keypoints of inference result.
    '''
    def __init__(self, resuls: Results = None):
        '''L,Rは画面の観察者からみた方向'''
        self.keypoints = {
            'eye_r': None,
            'eye_l': None,
            'nose': None,
            'ear_r': None,
            'ear_l': None,
            'shoulder_r': None,
            'shoulder_l': None,
            'elbow_r': None,
            'elbow_r': None,
            'wrist_r': None,
            'wrist_l': None,
            'waist_r': None,
            'waist_l': None,
            'knee_r': None,
            'knee_l': None,
            'ankle_r': None,
            'ankle_l': None,
        }
        # self.infer_time = None
        self.body_tilt = None
        self.arm_tilt = None
        if resuls != None:
            self.update(resuls)

    def update(self, results: Results):
        # if no inference result, dont update
        if len(results) <= 0:
            return
        results = results[0]
        # print(results)
        keypoints = results.keypoints
        speed = results.speed
        # self._update_time(speed)
        self._update_keypoints(keypoints)
        self._update_tilt()
        self._update_angle()

    def _update_tilt(self):
        '''tilt is calculated by shoulder's angle'''
        # dont update if shoulders are out of img
        if self.keypoints['shoulder_r'] == [0.0, 0.0] or self.keypoints['shoulder_l'] == [0.0, 0.0]:
            return
        x_delta = self.keypoints['shoulder_r'][0] - self.keypoints['shoulder_l'][0]
        y_delta = self.keypoints['shoulder_r'][1] - self.keypoints['shoulder_l'][1]
        self.body_tilt = math.sin(math.atan2(y_delta, x_delta))

    def _update_angle(self):
        def get_angle(to_mid, to_elbow):
            
            cross = np.cross(to_mid, to_elbow)
            sin = cross / (np.linalg.norm(to_mid) * np.linalg.norm(to_elbow))
            if np.isnan(sin):
                sin = 0
            return sin
        
        shoulder_r = np.array(self.keypoints['shoulder_r'])
        shoulder_l = np.array(self.keypoints['shoulder_l'])
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

    # def _update_time(self, speed):
    #     '''this method is under implemention...'''
    #     # total_time = speed['preprocess'] + speed['inference'] + speed['postprocess']
    #     # self.infer_time = total_time

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
        self.keypoints['ankle_r'] = xyn[16]




def main(bodyinfo: BodyInfo):
    ret, img = cam.read()
    img = cv2.resize(img, (IMG_W, IMG_H))
    img = cv2.flip(img, 1) # flip LR, we got mirror like img
    
    if(ret):
        result = model.predict(
            img,
            imgsz=(IMG_H, IMG_W),
            device='cpu',
            verbose=False,
        )[0]
        bodyinfo.update(result)
        plot_img = result.plot(
            kpt_radius=1,
            boxes=False,
        )
        # cv2.imshow('camImg', plot_img)
        
        if bodyinfo.body_tilt == None:
            tilt = 0
        else:
            tilt = bodyinfo.body_tilt
        if bodyinfo.arm_tilt == None:
            angle = 0
        else:
            angle = bodyinfo.arm_tilt
        
        # pyautogui.moveTo(
        #     WINDOW_WIDTH / 2 + tilt * MOUSE_MOVE_COEF_H,
        #     WINDOW_HEIGHT / 2 - angle * MOUSE_MOVE_COEF_V,
        #     _pause = False,
        # )
        
        mouse.move(
            WINDOW_WIDTH / 2 + tilt * MOUSE_MOVE_COEF_H,
            WINDOW_HEIGHT / 2 - angle * MOUSE_MOVE_COEF_V
        )
        # print(tilt, angle)
    if cv2.waitKey(1) & 0xFF == ord('q') or keyboard.is_pressed('ctrl+q'):
        return -1
    return 0

print("LOADING MODELS...")
model = YOLO('yolov8n-pose')
print("CONNECTING TO CAMERA...")
cam = cv2.VideoCapture(0)

print(f"START INFERENCE ON IMG({IMG_W}, {IMG_H})...")


p = psutil.Process(os.getpid())
p.cpu_affinity([0,1,2])

bodyinfo = BodyInfo()

while(True):
    flg = main(bodyinfo)
    if flg == -1:
        break
cam.release()
cv2.destroyAllWindows()

print('finish')


# results = model('https://ultralytics.com/images/bus.jpg')

# for result in results:
#     boxes = result.boxes  # Boxes object for bounding box outputs
#     masks = result.masks  # Masks object for segmentation masks outputs
#     keypoints = result.keypoints  # Keypoints object for pose outputs
#     probs = result.probs  # Probs object for classification outputs
#     obb = result.obb  # Oriented boxes object for OBB outputs
#     result.show()  # display to screen
#     result.save(filename='result.jpg')  # save to disk
from datetime import datetime
from time import sleep

from ctypes import windll

import os


def main():
	hour, minute = 7, 0

	time = input("關機時間(7:00)：")

	if len(time) > 0:
		if ":" not in time:
			time = time + ":00"

		h, m = time.split(":")

		hour = int(h)
		minute = int(m)

	while True:
		handle = windll.user32.FindWindowW(None, "劍靈")
		now = datetime.now()

		if (now.hour == hour and now.minute >= minute) or (handle == 0):
			os.system("shutdown /s /f /t 1")

		sleep(60)



if __name__ == '__main__':
	main()
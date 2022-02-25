#SingleInstance force

main() {
	Sleep, 1000
	getIn()
	getToM1()
	getToM2()
	getToM3()
	getOut()
}


getOut() {
	runForward(3000)

	Send, {w down}

	Loop {
		PixelGetColor, color, 1126, 775
		if (color=0x00DFFF) {
			Send, {w up}
			Loop, 50 {
				Send, fy
				Sleep, 100
			}
			Break
		}
	}

	Sleep, 20000
}

getToM3() {
	runForward(9000)

	Send, {d down}
	Sleep, 2000
	Send, {d up}

	Send, {a down}
	Sleep, 280
	Send, {a up}

	Send, +{w down}
	Sleep, 6000
	Send, {space down}
	Sleep, 2000
	Send, {w up}{space up}
	Sleep, 5000

	Send, {w down}
	Sleep, 500
	Send, {w up}

	bossFight()

	waitForRun(8000)

	; 撿東西
	Sleep, 1000
	Send, {w down}{d down}
	Sleep, 200
	Send, {d up}
	Sleep, 200
	Send, {w up}
	Send, {f down}
	Sleep, 1000
	Send, {f up}

	; 撿材料箱
	Send, n
	Sleep, 300
	Send, y
}

getToM2() {
	Send, +{w down}
	Sleep, 8800

	Send, {space down}
	Sleep, 2000
	Send, {w up}{space up}
	Sleep, 1000

	Send, {w down}
	Sleep 500
	Send, {w up}

	bossFight()

	; 脫戰
	waitForRun(8000)
}

getIn() {
	Send, {s 4}
	Send, {s down}
	Sleep, 8000
	Send, {s up}	
	Sleep, 15000
}

getToM1() {
	; 跑到門口
	runForward(11300)

	; 左轉
	Send, {a down}
	Sleep, 7400
	Send, {a up}

	Send, {w down}
	Sleep, 1000
	Send, {w up}


	; 打小怪
	normalFight()

	; 等脫戰
	waitForRun(8000)
}

bossFight() {
	Send, e
	Sleep, 500

	Send, +{e 10}
	Sleep, 500

	pressKey("z")
	pressKey("2")
	pressKey("v")
	pressKey("4")

	Click, down, right

	count := 0
	Loop {
		if (isKuangBaoReady()) {
			pressKey("Tab")
		}
		Send, v4e
		Sleep, 30

		if (!isMonsterLive()) {
			count++
		} else if (count>0) {
			count--
		}

		if (count > 3) {
			Break
		}
	}

	Click, up, right

	Sleep, 2000
	if (isKuangBaoOn()) {
		pressKey("Tab")
	}
}

normalFight() {
	pressKey("2")
	pressKey("v")
	pressKey("4")

	Click, down, right

	count := 0
	Loop {
		Send, v4e
		Sleep, 30

		if (!isMonsterLive()) {
			count++
		} else if (count>0) {
			count--
		}

		if (count > 3) {
			Break
		}
	}

	Click, up, right
}

waitForRun(t=15000) {
	Sleep, t
}

pressKey(k) {
	Send, {%k% down}
	Sleep, 500
	Send, {%k% up}
	Sleep, 100
}

runForward(t) {
	Send, +{w down}
	Sleep, t
	Send, {w up}
	Sleep, 2000
}


shutdownIfError() {
	count := 0

	Loop, 5 {
		if (!isInPosition()) {
			count++
		} else if (count > 0) {
			count--
		}
		Sleep, 1000
	}

	if (count > 3) {
		Send, {PrintScreen}
		Shutdown, 13
	}
}

isInPosition() {
	return getColor(1636, 297)=0x9CA6CA
}

getColor(x, y) {
	PixelGetColor, color, x, y

	return color
}

isMonsterLive() {
	return (getColor(797, 83)=0xFFFFFF) || (getColor(763, 75)=0xF1F1F2)
}

isKuangBaoOn() {
	return getColor(843, 929)=0x625489
}

isKuangBaoReady() {
	return getColor(845, 926)=0x453292
}


!j::
	Loop {
		main()
		if (A_Args.Length() > 0) {
			shutdownIfError()
		} else if (!isInPosition()) {
			Loop {
				SoundPlay, *16
				Sleep, 1000
			}
		}
	}
	return

!k::
	ExitApp


!1::
	Sleep, 1000
	
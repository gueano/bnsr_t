function OnEvent(event, arg)
    if (event == "MOUSE_BUTTON_RELEASED" and arg == 4) then
        PressAndReleaseKey("v")
        Sleep(300)
        for i = 0, 5 do
            MoveMouseRelative(110, 0)
            Sleep(10)
        end
    end
end
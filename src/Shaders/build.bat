set INCLUDEPATH="%WindowsSdkDir%\Include\%WindowsSDKVersion%\um"

fxc ShockWave.hlsl /nologo /T lib_4_0_level_9_3_ps_only /D D2D_FUNCTION /D D2D_ENTRY=main /Fl ShockWave.fxlib /I %INCLUDEPATH%

fxc ShockWave.hlsl /nologo /T ps_4_0_level_9_3 /D D2D_FULL_SHADER /D D2D_ENTRY=main /E main /setprivate ShockWave.fxlib /Fo:ShockWave.bin /I %INCLUDEPATH%

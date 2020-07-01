#NoTrayIcon

#pragma compile(FileDescription, '简单调用Windows自带复制对话框复制文件或文件夹')
#pragma compile(FileVersion, 1.0.0.0)
#pragma compile(ProductName, Leo)
#pragma compile(ProductVersion, 1.0.0.0)
#pragma compile(LegalCopyright, Copyright ©  2020)
#pragma compile(x64, false)
#pragma compile(UPX, true)
#pragma compile(Compression, 9)
#pragma compile(Console, true)

#include <APIShellExConstants.au3>
#include <WinAPIFiles.au3>
#include <WinAPIShellEx.au3>

Global $bOverWrite = False, $bHelp = False
Global $srcPath = '', $dstPath = ''


For $i = 1 To $CmdLine[0]
    If $CmdLine[$i] = '/y' Then
        $bOverWrite = True
    ElseIf $CmdLine[$i] = '/?' Then
        $bHelp = True
    ElseIf $srcPath = '' Then
        $srcPath = $CmdLine[$i]
    ElseIf $dstPath = '' Then
        $dstPath = $CmdLine[$i]
    EndIf
Next


If $bHelp Or $srcPath = '' Or $dstPath = '' Then
    ConsoleWrite("调用Windows自带的复制文件对话框来复制文件。" & @CRLF & @CRLF & _
                 "CopyX [/Y] source destination" & @CRLF & @CRLF & @TAB & _
                 "/Y             不提示直接覆盖目标文件。" & @CRLF & @TAB & _
                 "source         指定要复制的文件。" & @CRLF & @TAB & _
                 "destination    为新文件指定目录和/或文件名。" & @CRLF)
    Exit -1
EndIf

If FileExists($srcPath) <> 1 Then
    ConsoleWriteError('复制失败, 源文件不存在!')
    Exit -1
EndIf

If $bOverWrite Then
     _WinAPI_ShellFileOperation($srcPath, $dstPath, $FO_COPY, BitOR($FOF_NOCONFIRMATION, $FOF_NOCONFIRMMKDIR))
     Global $ret = @extended
Else
     _WinAPI_ShellFileOperation($srcPath, $dstPath, $FO_COPY, $FOF_SIMPLEPROGRESS)
     Global $ret = @extended
EndIf


If $ret <> 0 Then
    ConsoleWriteError("复制失败,错误代码: " & $ret & " 参考: https://www.autoitscript.com/autoit3/docs/libfunctions/_WinAPI_ShellFileOperation.htm")
    Exit $ret
Else
    Exit 0
EndIf

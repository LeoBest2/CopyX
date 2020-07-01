using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace CopyX
{
    class Program
    {
        public enum FO_Func : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FO_Func wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;

        }


        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In, Out] ref SHFILEOPSTRUCT lpFileOp);

        static string usage = "调用Windows自带的复制文件对话框来复制文件。\n" +
                        "\n CopyX [/Y] source destination\n\n" +
                        "\t/Y         \t不提示直接覆盖目标文件。\n" +
                        "\tsource     \t指定要复制的文件。\n" +
                        "\tdestination\t为新文件指定目录和/或文件名。";


        static int Main(string[] args)
        {
            bool bOverWrite = false;
            string srcPath = null, dstPath = null;
            foreach (string arg in args)
            {
                if (arg == "/y" || arg == "/Y")
                {
                    bOverWrite = true;
                }
                else if (srcPath == null)
                {
                    srcPath = arg;
                }
                else if (dstPath == null)
                {
                    dstPath = arg;
                }
            }

            if (args.Contains("/?") || srcPath == null || dstPath == null)
            {
                Console.WriteLine(usage);
                return -1;
            }

            if ((!File.Exists(srcPath)) && (!Directory.Exists(srcPath)))
            {
                Console.WriteLine("复制失败, 源文件不存在!");
                return -1;
            }


            SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT();
            lpFileOp.hwnd = IntPtr.Zero;
            lpFileOp.wFunc = FO_Func.FO_COPY;
            lpFileOp.pFrom = srcPath + '\0';
            lpFileOp.pTo = dstPath + '\0';
            /*
            FOF_MULTIDESTFILES = 0x0001
            FOF_CONFIRMMOUSE = 0x0002
            FOF_SILENT = 0x0004                 // don't create progress/report
            FOF_RENAMEONCOLLISION = 0x0008
            FOF_NOCONFIRMATION = 0x0010         // Don't prompt the user.
            FOF_WANTMAPPINGHANDLE = 0x0020      // Fill in SHFILEOPSTRUCT.hNameMappings
            FOF_ALLOWUNDO = 0x0040
            FOF_FILESONLY = 0x0080              // on *.* do only files
            FOF_SIMPLEPROGRESS = 0x0100         // means don't show names of files
            FOF_NOCONFIRMMKDIR = 0x0200         // don't confirm making any needed dirs
            FOF_NOERRORUI = 0x0400              // don't put up error UI
            FOF_NOCOPYSECURITYATTRIBS = 0x0800  // dont copy NT file Security Attributes
            FOF_NORECURSION = 0x1000            // don't recurse into directories.
            FOF_NO_CONNECTED_ELEMENTS = 0x2000  // don't operate on connected elements.
            FOF_WANTNUKEWARNING = 0x4000        // during delete operation warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            FOF_NORECURSEREPARSE = 0x8000       // treat reparse points as objects not containers
            */
            lpFileOp.fFlags = bOverWrite ? (ushort)(0x0010 | 0x0200) : (ushort)0x0100;
            lpFileOp.hNameMappings = IntPtr.Zero;
            lpFileOp.fAnyOperationsAborted = false;

            int ret = SHFileOperation(ref lpFileOp);
            switch (ret)
            {
                case 0x00: return 0;  // 拷贝成功
                case 0x71: Console.WriteLine("The source and destination files are the same file."); break;
                case 0x72: Console.WriteLine("Multiple file paths were specified in the source buffer, but only one destination file path."); break;
                case 0x73: Console.WriteLine("Rename operation was specified but the destination path is a different directory. Use the move operation instead."); break;
                case 0x74: Console.WriteLine("The source is a root directory, which cannot be moved or renamed."); break;
                case 0x75: Console.WriteLine("The operation was canceled by the user, or silently canceled if the appropriate flags were supplied to SHFileOperation."); break;
                case 0x76: Console.WriteLine("The destination is a subtree of the source."); break;
                case 0x78: Console.WriteLine("Security settings denied access to the source."); break;
                case 0x79: Console.WriteLine("The source or destination path exceeded or would exceed MAX_PATH."); break;
                case 0x7A: Console.WriteLine("The operation involved multiple destination paths, which can fail in the case of a move operation."); break;
                case 0x7C: Console.WriteLine("The path in the source or destination or both was invalid."); break;
                case 0x7D: Console.WriteLine("The source and destination have the same parent folder."); break;
                case 0x7E: Console.WriteLine("The destination path is an existing file."); break;
                case 0x80: Console.WriteLine("The destination path is an existing folder."); break;
                case 0x81: Console.WriteLine("The name of the file exceeds MAX_PATH."); break;
                case 0x82: Console.WriteLine("The destination is a read-only CD-ROM, possibly unformatted."); break;
                case 0x83: Console.WriteLine("The destination is a read-only DVD, possibly unformatted."); break;
                case 0x84: Console.WriteLine("The destination is a writable CD-ROM, possibly unformatted."); break;
                case 0x85: Console.WriteLine("The file involved in the operation is too large for the destination media or file system."); break;
                case 0x86: Console.WriteLine("The source is a read-only CD-ROM, possibly unformatted."); break;
                case 0x87: Console.WriteLine("The source is a read-only DVD, possibly unformatted."); break;
                case 0x88: Console.WriteLine("The source is a writable CD-ROM, possibly unformatted."); break;
                case 0xB7: Console.WriteLine("MAX_PATH was exceeded during the operation."); break;
                case 0x402: Console.WriteLine("An unknown error occurred. This is typically due to an invalid path in the source or destination. This error does not occur on Windows Vista and later."); break;
                default: Console.WriteLine("复制失败, 未知错误代码【" + ret + "】，参考: https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shfileoperationa"); break;
            }
            return ret;
        }
    }
}

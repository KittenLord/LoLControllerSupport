using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LoLController
{
    internal static class KeyTable
    {
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, ushort uMapType, IntPtr dwhkl); 
        private static ushort FromScancodeToVirtualkey(ushort code)
        {
            var key = MapVirtualKeyEx(code, 1, (IntPtr)null);

            return (ushort)key;
        }

        private static ushort FromVirtualkeyToScancode(ushort code)
        {
            var key = MapVirtualKeyEx(code, 0, (IntPtr)null);

            return (ushort)key;
        }


        private static Dictionary<ushort, ushort> VirtualToScanKeys = new Dictionary<ushort, ushort>()
        {

            { 17 ,29 }  ,
            { 91 ,91  } ,
            { 32 ,57  } ,
            { 18 ,56  } ,
            { 90 ,44  } ,
            { 88 ,45  } ,
            { 67 ,46  } ,
            { 86 ,47  } ,
            { 66 ,48  } ,
            { 78 ,49  } ,
            { 77 ,50  } ,
            { 188, 51 } ,
            { 190, 52 } ,
            { 191, 53 } ,
            { 16 ,42  } ,
            { 20 ,58  } ,
            { 65 ,30  } ,
            { 83 ,31  } ,
            { 68 ,32  } ,
            { 70 ,33  } ,
            { 71 ,34  } ,
            { 72 ,35  } ,
            { 74 ,36  } ,
            { 75 ,37  } ,
            { 76 ,38  } ,
            { 186, 39 } ,
            { 222, 40 } ,
            { 220, 43 } ,
            { 13 ,28  } ,
            { 9 ,15   } ,
            { 81 ,16  } ,
            { 87 ,17  } ,
            { 69 ,18  } ,
            { 82 ,19  } ,
            { 84 ,20  } ,
            { 89 ,21  } ,
            { 85 ,22  } ,
            { 73 ,23  } ,
            { 79 ,24  } ,
            { 80 ,25  } ,
            { 219, 26 } ,
            { 221, 27 } ,
            { 8 ,14   } ,
            { 187 ,13 } ,
            { 189 ,12 } ,
            { 48 ,11  } ,
            { 57 ,10  } ,
            { 56 ,9   } ,
            { 55 ,8   } ,
            { 54 ,7   } ,
            { 53 ,6   } ,
            { 52 ,5   } ,
            { 51 ,4   } ,
            { 50 ,3   } ,
            { 49 ,2   } ,
            { 192, 41 } ,
            { 27 ,1   } ,
            { 112, 59 } ,
            { 113, 60 } ,
            { 114, 61 } ,
            { 115, 62 } ,
            { 116, 63 } ,
            { 117, 64 } ,
            { 118, 65 } ,
            { 119, 66 } ,
            { 120, 67 } ,
            { 121, 68 } ,
            { 122, 87 } ,
            { 123, 88 } ,
            { 145, 70 } ,
            { 19 ,0   } ,
            { 36 ,71  } ,
            { 33 ,73  } ,
            { 34 ,81  } ,
            { 37 ,75  } ,
            { 38 ,72  }
        };
    
        public static ushort GetScanKey(ushort virtualKey)
        {
            if(VirtualToScanKeys.ContainsKey(virtualKey)) return VirtualToScanKeys[virtualKey];

            return FromVirtualkeyToScancode(virtualKey);
        }

        public static ushort GetVirtualKey(ushort scanKey)
        {
            var contains = VirtualToScanKeys.ToList().Any(p => p.Value == scanKey);

            if (contains) return VirtualToScanKeys.ToList().Find(p => p.Value == scanKey).Key;

            return FromScancodeToVirtualkey(scanKey);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace NPOI
{
    public enum HeadIdx
    {
        jp,
        trans,
        trans_jd,
        i,
        j,
        SheetName,

        Count
    }
    public class ExcelHead
    {
        public int[] hidx;

        public int this[HeadIdx i]
        {
            get { return hidx[(int)i]; }
        }

        public ExcelHead(IRow hrow)
        {
            hidx = new int[(int)HeadIdx.Count];
            for(int i = 0; i < hrow.LastCellNum; ++i)
            {
                var v = hrow.Cell(i);
                for(int j = 0; j < (int)HeadIdx.Count; ++j)
                {
                    if(v.StringCellValue == ((HeadIdx)j).ToString())
                    {
                        hidx[j] = i;
                        break;
                    }
                }
            }

        }
    }
}

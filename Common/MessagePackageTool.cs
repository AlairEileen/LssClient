using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class MessagePackageTool
    {

        ///数据头
        const string header = "+-cmd-+",
                ///数据尾
                footer = "-+end+-";
        private static byte[] HeaderByte { get => Encoding.ASCII.GetBytes(header); }
        private static byte[] FooterByte { get => Encoding.ASCII.GetBytes(footer); }
        public static byte[] ToPackage(this Msg messageModel)
        {
            var msgB = messageModel.ToMessageByte();
            var pakB = new byte[msgB.Length + HeaderByte.Length + FooterByte.Length];
            HeaderByte.CopyTo(pakB, 0);
            msgB.CopyTo(pakB, HeaderByte.Length);
            FooterByte.CopyTo(pakB, pakB.Length - FooterByte.Length);
            return pakB;
        }

        public static byte[][] GetMessageByte(this byte[] dataO,int dataSize)
        {
            var data = dataO.Take(dataSize).ToArray();
            ///数据包头部下标集合
            var bheaderLastIndexs = new List<int>();
            ///数据包尾部下标集合
            var bFooterFirstIndexs = new List<int>();
            ///找到所有数据包的头部和尾部
            for (int i = 0; i < data.Length; i++)
            {
                if (HasData(HeaderByte, data, i))
                {
                    i = i + HeaderByte.Length - 1;
                    bheaderLastIndexs.Add(i + 1);
                }
                if (HasData(FooterByte, data, i))
                {
                    bFooterFirstIndexs.Add(i);
                    i = i + FooterByte.Length - 1;
                }
            }
            ///将数据包去除头部和尾部的数据包列表
            var dataList = new List<byte[]>();
            if (bheaderLastIndexs.Count == bFooterFirstIndexs.Count)
            {
                for (int i = 0; i < bheaderLastIndexs.Count; i++)
                {
                    dataList.Add(data.Skip(bheaderLastIndexs[i]).Take(bFooterFirstIndexs[i] - bheaderLastIndexs[i]).ToArray());
                }
            }
            else///如果头部数量不等于尾部数量则为错误数据，不进行处理
                return null;
            return dataList.ToArray();
        }

        /// <summary>
        /// 判断是否为数据头或者尾部
        /// </summary>
        /// <param name="bHeader"></param>
        /// <param name="bSum"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static bool HasData(byte[] bHeader, byte[] bSum, int index)
        {
            var flag = true;
            for (int i = 0; i < bHeader.Length; i++)
            {
                if (bHeader[i] != bSum[index + i])
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELWork
{
    /// <summary>HD-Copy IMG檔案處理相關類別</summary>
    public static class ImgConvert
    {
        /// <summary>解碼IMG檔案成標準磁碟映像(VFD)。</summary>
        /// <param name="ImgFile">IMG檔案的位元組陣列。</param>
        /// <returns>標準磁碟映像(VFD)的位元組陣列。</returns>
        public static byte[] DecodeIMG(byte[] ImgFile)
        {
            try
            {
                byte[] tmpFmtInfo = new byte[2];
                byte[] tmpImgContent;

                List<byte> tmpResult = new List<byte>();

                Array.Copy(ImgFile, 0, tmpFmtInfo, 0, 2);

                // 分辨HDCopy 1.7前和2.0後的格式，取得主要的內容
                if (BitConverter.ToString(tmpFmtInfo) == BitConverter.ToString(new byte[2] { 0xFF, 0x18 }))
                {
                    tmpImgContent = new byte[ImgFile.Length - 0x0E];
                    Array.Copy(ImgFile, 0x0E, tmpImgContent, 0x0, tmpImgContent.Length);
                }
                else
                {
                    tmpImgContent = ImgFile;
                }

                // 開始解碼
                int offset = 0x0;
                int maxTrackLen = 0; // 最大Track數，從0開始
                int secPerTrack = 18; // 每Track的Sector數，2HD為18
                bool[] notEmptyTrack = new bool[168]; // 每個Head是否有壓縮資料，每Track兩個Head

                // 0x00 取得最大Track數
                maxTrackLen = tmpImgContent[offset++];

                // 0x01 取得每Track的Sector數
                secPerTrack = tmpImgContent[offset++];

                // 0x02~0xA9 取得每個Head是否有壓縮資料
                for (int h = 0; h < 168; h++)
                {
                    notEmptyTrack[h] = Convert.ToBoolean(tmpImgContent[offset + h]);
                }
                offset += 168;

                // 0xAA~ 解碼RLE壓縮的內容
                for (int t = 0; t <= maxTrackLen; t++) // 跑每個Track
                {
                    // 跑每個Head，一個Track有兩個Head
                    for (int h = 0; h < 2; h++)
                    {
                        // 如果這個Head沒有資料，則跳過
                        if (!notEmptyTrack[(t * 2) + h])
                        {
                            tmpResult.AddRange(new byte[0x200 * secPerTrack]);
                            continue;
                        }

                        // 取得這個Head壓縮後的資料長度
                        int tmpDataLen = 0;
                        tmpDataLen = BitConverter.ToInt16(tmpImgContent, offset);
                        offset += 2;

                        // 將壓縮資料解碼
                        byte escByte = 0x0; // RLE壓縮的Escape Byte
                        for (int l = 0; l < tmpDataLen; l++)
                        {
                            if (l == 0)
                                escByte = tmpImgContent[offset]; // 每個資料塊的第一個位元組紀錄Escape Byte
                            else
                            {
                                // 遇到Escape Byte時
                                if (tmpImgContent[offset + l] == escByte)
                                {
                                    l++;
                                    byte repeatByte = tmpImgContent[offset + l++];
                                    int repeatLen = tmpImgContent[offset + l];

                                    for (int r = 0; r < repeatLen; r++)
                                    {
                                        // 將解碼的位元加入tmpResult
                                        tmpResult.Add(repeatByte);
                                    }
                                }
                                else
                                {
                                    // 將解碼的位元加入tmpResult
                                    tmpResult.Add(tmpImgContent[offset + l]);
                                }
                            }
                        }

                        offset += tmpDataLen;
                    }
                }

                return tmpResult.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("處理IMG時錯誤，可能IMG格式不正確，請檢查來源檔案是否正常。", new Exception(ex.Message));
            }
        }

        /// <summary>取得標準磁碟映像(VFD)內的檔案，僅會取得根目錄的檔案。</summary>
        /// <param name="VfdFile">標準磁碟映像(VFD)的位元組陣列。</param>
        /// <returns>檔案的位元組陣列集合，Key為檔名。</returns>
        public static Dictionary<string, byte[]> GetFilesFromVfd(byte[] VfdFile)
        {
            try
            {
                Dictionary<string, byte[]> tmpFileCollection = new Dictionary<string, byte[]>();

                int offset = 0x0;
                int sectorSize = 0x200; // 每個Sector占用的空間大小，統一為512byte
                int clusterSize = sectorSize; // 每個cluster占用的大小，2HD為1 Sector
                int bootSector = 1; // 開機磁區占用的Sector數
                int fatSector = 9; // FAT12格式化占用的Sector數
                int directorySector = 14; // 根目錄的檔案/資料夾進入點所占用的Sector數

                // 根據映像大小，決定FAT各個資料長度參數
                if (VfdFile.Length == 1440 * 1024)
                {
                    fatSector = 9;
                }
                else if (VfdFile.Length == 1200 * 1024)
                {
                    fatSector = 7;
                }
                else if (VfdFile.Length == 720 * 1024)
                {
                    fatSector = 3;
                    clusterSize = sectorSize * 2;
                    directorySector = 7;
                }
                else if (VfdFile.Length == 640 * 1024)
                {
                    fatSector = 2;
                    clusterSize = sectorSize * 2;
                    directorySector = 7;
                }
                else
                {
                    throw new Exception("磁碟映像的大小不正確");
                }

                // 從Root Directory Entries取得檔案列表
                offset = sectorSize * (bootSector + (fatSector * 2));
                for (int d = 0; d < directorySector; d++)
                {

                }

                return tmpFileCollection;
            }
            catch (Exception ex)
            {
                throw new Exception("處理VFD檔案時發生錯誤，請確認傳入的映像格式正確，並為FAT12格式。", new Exception(ex.Message));
            }
        }
    }
}

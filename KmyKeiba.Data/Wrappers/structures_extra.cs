using System.Text;

#nullable disable

namespace KmyKeiba.JVLink.Wrappers.JVLib
{
  public static partial class JVData_Struct
  {

    #region 32.ウッドチップ調教


    public struct JV_WC_WOODTIP
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public string TresenKubun;  // トレセン区分

      public YMD ChokyoDate;      // 調教年月日
      public string ChokyoTime;   // 調教時刻
      public string KettoNum;     // 血統登録番号
      public string CourseCD;     // コース
      public string Baba;         // 馬場周り
      public string Reserved;

      public string HaronTime10;  // ハロンタイム合計
      public string LapTime10;    // ラップタイム
      public string HaronTime9;   // ハロンタイム合計
      public string LapTime9;     // ラップタイム
      public string HaronTime8;   // ハロンタイム合計
      public string LapTime8;     // ラップタイム
      public string HaronTime7;   // ハロンタイム合計
      public string LapTime7;     // ラップタイム
      public string HaronTime6;   // ハロンタイム合計
      public string LapTime6;     // ラップタイム
      public string HaronTime5;   // ハロンタイム合計
      public string LapTime5;     // ラップタイム
      public string HaronTime4;   // ハロンタイム合計
      public string LapTime4;     // ラップタイム
      public string HaronTime3;   // ハロンタイム合計
      public string LapTime3;     // ラップタイム
      public string HaronTime2;   // ハロンタイム合計
      public string LapTime2;     // ラップタイム
      public string LapTime1;     // ラップタイム
      public string crlf;         // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[60];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        TresenKubun = MidB2S(ref bBuff, 12, 1);
        ChokyoDate.SetDataB(MidB2B(ref bBuff, 13, 8));
        ChokyoTime = MidB2S(ref bBuff, 21, 4);
        KettoNum = MidB2S(ref bBuff, 25, 10);
        CourseCD = MidB2S(ref bBuff, 35, 1);
        Baba = MidB2S(ref bBuff, 36, 1);
        Reserved = MidB2S(ref bBuff, 37, 1);

        HaronTime10 = MidB2S(ref bBuff, 38, 4);
        LapTime10 = MidB2S(ref bBuff, 42, 3);
        HaronTime9 = MidB2S(ref bBuff, 45, 4);
        LapTime9 = MidB2S(ref bBuff, 49, 3);
        HaronTime8 = MidB2S(ref bBuff, 52, 4);
        LapTime8 = MidB2S(ref bBuff, 56, 3);
        HaronTime7 = MidB2S(ref bBuff, 59, 4);
        LapTime7 = MidB2S(ref bBuff, 63, 3);
        HaronTime6 = MidB2S(ref bBuff, 66, 4);
        LapTime6 = MidB2S(ref bBuff, 70, 3);
        HaronTime5 = MidB2S(ref bBuff, 73, 4);
        LapTime5 = MidB2S(ref bBuff, 77, 3);
        HaronTime4 = MidB2S(ref bBuff, 80, 4);
        LapTime4 = MidB2S(ref bBuff, 84, 3);
        HaronTime3 = MidB2S(ref bBuff, 87, 4);
        LapTime3 = MidB2S(ref bBuff, 91, 3);
        HaronTime2 = MidB2S(ref bBuff, 94, 4);
        LapTime2 = MidB2S(ref bBuff, 98, 3);
        LapTime1 = MidB2S(ref bBuff, 101, 3);
        crlf = MidB2S(ref bBuff, 104, 2);
        bBuff = null;
      }
    }

    #endregion
  }
}

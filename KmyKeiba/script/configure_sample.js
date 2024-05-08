
(function () {

  // キャッシュするレースの最大数。RAM容量が気になる場合は減らし、逆に余裕があれば増やしてもよい
  appconfig.raceCacheMax = 48;

  // 傾向検索、分析画面などで絞り込み条件に「距離」を指定した場合に同じ距離として扱う範囲を指定する
  // 50を指定した場合、1600mレースでは1550 - 1650mのレースがヒットする
  // 中央レースはレース回数の少ない馬が多く、地方レースでは距離が10刻みになることもあるため、中央と地方で設定項目を分けている
  // InHorseGradeのついてないほう: 騎手、調教師、レース傾向などから検索した場合の設定
  // InHorseGradeのついているほう: その馬自身の成績から検索した場合の設定
  appconfig.distanceDiffCentral = 50;
  appconfig.distanceDiffCentralInHorseGrade = 50;
  appconfig.distanceDiffLocal = 50;
  appconfig.distanceDiffLocalInHorseGrade = 50;

  // 分析画面で分析に利用するデータの量。多いほどキャッシュのときにRAM容量を使う
  appconfig.analysisTableSourceSize = 500;

  // 分析画面で分析に利用するデータの量（useRaceHorseAnalyzerの場合）
  // useRaceHorseAnalyzerは１つのレースで複数の馬のデータを利用するが、馬の数だけデータの量にカウントされるので
  // レース単位でカウントされる他のAnalyzerより実際に含まれるレースの数が少なくなる
  appconfig.analysisTableRaceHorseSourceSize = 4000;

  // 分析画面で数字をクリックしたときに表示される過去レースリストの最大項目数
  appconfig.analysisTableSampleSize = 10;

  // 明日以降のレース予定をダウンロードする間隔を分で指定する
  // なおこれとは別に日付の変わり目に必ずダウンロードが実行される（自動更新をONにしている限り）
  appconfig.downloadNormalDataIntervalMinutes = 120;

  // 起動時のメッセージを表示するか（true/false）
  appconfig.isFirstMessageVisible = true;

  // 拡張メモにおける馬メモのグループ数。1未満を指定した場合は1を下限とする
  appconfig.expansionMemoGroupSize = 8;
});

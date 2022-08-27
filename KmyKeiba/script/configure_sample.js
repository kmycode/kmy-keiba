
(function () {
  //
  // 分析画面の項目
  // ※そのレースの発走より前のデータが集計されます
  //
  // [value] --- place        : 複勝率
  //             win          : 単勝率
  //             shortesttime : 最短タイム
  //             time         : タイム偏差値平均
  //             a3htime      : 後3ハロンタイム偏差値平均
  //             recovery     : 単勝回収率
  // [target] --- frame        : 枠番
  //              age          : 年齢
  //              color        : 色
  //              sex          : 性別
  //              runningstyle : 脚質
  //              popular      : 人気
  // [keys] --- kmykeiba.jsを参照。Similar系メソッドに指定する絞り込み条件と同一
  //
  // useHorseAnalyzer([name], [keys], [value])
  // useRaceHorseAnalyzer([name], [target], [keys], [value])
  // useRiderAnalyzer([name], [keys], [value])
  // useTrainerAnalyzer([name], [keys], [value])
  // useBloodAnalyzer([name], [relation], [keys], [value])
  // useFinder([name], [keys], [value])
  const horseTable = appconfig.createAnalysisTable('馬');
  horseTable.useHorseAnalyzer('全レース複勝', '', 'place');
  horseTable.useHorseAnalyzer('全レースタイム偏差値', '', 'time');
  horseTable.useHorseAnalyzer('同競馬場複勝', 'course', 'place');
  horseTable.useHorseAnalyzer('馬場状態複勝', 'condition', 'place');
  horseTable.useHorseAnalyzer('馬場状態タイム偏差値', 'condition', 'time');
  horseTable.useHorseAnalyzer('天気複勝', 'weather', 'place');
  horseTable.useHorseAnalyzer('距離複勝', 'distance', 'place');
  horseTable.useHorseAnalyzer('距離タイム偏差値', 'distance', 'time');
  horseTable.useHorseAnalyzer('距離A3HT偏差値', 'distance', 'a3htime');
  horseTable.useHorseAnalyzer('間隔日数複勝', 'interval', 'place');
  const raceTable = appconfig.createAnalysisTable('競馬場');
  raceTable.useRaceHorseAnalyzer('枠番複勝', 'frame', 'course', 'place');
  raceTable.useRaceHorseAnalyzer('枠番距離複勝', 'frame', 'course|distance', 'place');
  raceTable.useRaceHorseAnalyzer('枠番距離馬場複勝', 'frame', 'course|distance|condition', 'place');
  raceTable.useRaceHorseAnalyzer('脚質複勝', 'runningstyle', 'course', 'place');
  raceTable.useRaceHorseAnalyzer('脚質距離馬場複勝', 'runningstyle', 'course|distance|condition', 'place');
  raceTable.useRaceHorseAnalyzer('年齢複勝', 'age', 'course', 'place');
  raceTable.useRaceHorseAnalyzer('人気複勝', 'popular', 'course', 'place');
  const riderTable = appconfig.createAnalysisTable('騎手');
  riderTable.useRiderAnalyzer('複勝', '', 'place');
  riderTable.useRiderAnalyzer('同競馬場複勝', 'course', 'place');
  riderTable.useRiderAnalyzer('馬場状態複勝', 'condition', 'place');
  riderTable.useRiderAnalyzer('距離複勝', 'distance', 'place');
  riderTable.useRiderAnalyzer('距離条件複勝', 'distance|subject', 'place');
  riderTable.useRiderAnalyzer('距条場状複勝', 'distance|subject|course|condition', 'place');
  const bloodTable = appconfig.createAnalysisTable('血統');
  bloodTable.useBloodAnalyzer('同父複勝', 'f', '', 'place');
  bloodTable.useBloodAnalyzer('同父距離複勝', 'f', 'distance', 'place');
  bloodTable.useBloodAnalyzer('同父地面距離複勝', 'f', 'distance|ground', 'place');
  bloodTable.useBloodAnalyzer('同母父複勝', 'mf', '', 'place');

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
  appconfig.analysisTableSourceSize = 1000;

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

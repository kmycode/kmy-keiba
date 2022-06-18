
(function () {
  //
  // 分析画面の項目
  //
  // [value] --- place   : 複勝率
  //             win     : 単勝率
  //             time    : タイム偏差値平均
  //             a3htime : 後3ハロンタイム偏差値平均
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
});

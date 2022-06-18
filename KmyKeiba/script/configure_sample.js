
(function() {
  const horseTable = appconfig.createAnalysisTable('馬');
  horseTable.useRaceHorseAnalyzer('全レース複勝', '', 'place');
  horseTable.useRaceHorseAnalyzer('全レースタイム偏差値', '', 'time');
  horseTable.useRaceHorseAnalyzer('同競馬場複勝', 'course', 'place');
  horseTable.useRaceHorseAnalyzer('馬場状態複勝', 'condition', 'place');
  horseTable.useRaceHorseAnalyzer('天気複勝', 'weather', 'place');
  horseTable.useRaceHorseAnalyzer('距離複勝', 'distance', 'place');
});

import { KmyKeiba } from 'official/kmykeiba.js';
import * as fs from 'official/njcompat_fs.js';

(async function () {
  // 一括実行であれば学習、それ以外の場合は予測をおこなう
  // 一括実行の場合も予測を行いたいときは、単にml = KmyKeiba.mlPrediction()とする
  const ml = KmyKeiba.isBulk() ? KmyKeiba.mlTraining() : KmyKeiba.mlPrediction();

  // レース取得
  const race = KmyKeiba.getTargetRaceWithResults();
  const horses = race.getHorses();

  // ばんえいではタイム偏差値が計算されないのでスキップする
  if (race.course === 83) {
    KmyKeiba.setBulkBanei(false);
    return;
  }

  for (const horse of horses) {
    // 学習データまたは予測データを作成する
    // mlTrainingもmlPredictionも同じaddRowメソッドを持っているので大丈夫
    ml.addRow([
      horse.history.timeDeviationValue / 150,
      horse.history.a3hTimeDeviationValue / 150,
      horse.history.runningStyle / 4,
    ], horse.place <= 3 ? 1 : 0);
  }

  // addRowしたデータを使い、結果を予測する
  // mlTrainingの場合、これは無視されて常に空の配列が返される
  const arr = await ml.predictAsync(false);
  if (arr.length === 0) {
    return;
  }

  // 予測結果が大きい順に並べ替えて、印や馬券の提案を行う
  const list = [];
  let i = 0;
  for (const horse of horses) {
    list.push({
      horse,
      point: arr[i++],
    });
  }
  const ranking = list.sort((a, b) => {
    if (a.point < b.point) return 1;
    if (a.point > b.point) return -1;
    return 0;
  });

  const suggestion = KmyKeiba.getSuggestion();
  suggestion.mark(ranking[0].horse.number, 1);
  suggestion.mark(ranking[1].horse.number, 2);
  suggestion.mark(ranking[2].horse.number, 3);
  suggestion.mark(ranking[3].horse.number, 4);
  suggestion.quinella(1, [ranking[0].horse.number, ranking[1].horse.number], [ranking[0].horse.number, ranking[1].horse.number, ranking[2].horse.number]);

  return JSON.stringify(arr);
});

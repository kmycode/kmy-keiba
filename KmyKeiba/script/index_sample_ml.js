import { KmyKeiba } from 'official/kmykeiba.js';
import * as fs from 'official/njcompat_fs.js';

(async function () {
  // 一括実行であれば学習、それ以外の場合は予測をおこなう
  // 一括実行の場合も予測を行いたいときは、単にml = KmyKeiba.mlPrediction()とする
  // 一括実行では両方、個別レースのスクリプト実行ではmlPredictionのみが正しく動作する
  const ml = KmyKeiba.isBulk() ? KmyKeiba.mlTraining() : KmyKeiba.mlPrediction();

  // レース取得
  // getTargetRaceはレース結果が取得できないのに対して、getTargetRaceWithResultsは結果（着順、タイムなど）を含めて取得可能
  // getTargetRaceWithResultsは一括実行でのみ正しく動作する。個別レースのスクリプト実行ではgetTargetRaceと同じ結果を返す
  // レース結果を機械学習のトレーニングの結果に利用する
  // ただし一度getTargetRaceWithResultsを使用すると、suggestion（印／買い目の提案）はスキップされる（一括実行に限る）
  // 一括実行で予測と収益集計を行いたい場合はgetTargetRaceに差し替えること
  const race = KmyKeiba.getTargetRaceWithResults();
  const horses = race.getHorses();

  // ばんえいではタイム偏差値が計算されないのでスキップする
  if (race.course === 83) {
    KmyKeiba.setBulkBanei(false);
    return;
  }

  for (const horse of horses) {
    // 学習データまたは予測データに追加する
    // mlTrainingもmlPredictionも同じaddRowメソッドを持っている。呼び出し方も同じでよいが、mlPredictionでは第２引数（結果）は無視される
    // 学習データはすべて0～1の範囲内、結果は0と1のどちらかにする（回帰分析の場合はまた変わる）
    // 一括実行において一回でもmlTrainingに対してaddRowすると、一括実行終了のタイミングで自動的に学習が開始される
    // なおmlTrainingは一括実行の場合のみ有効。個別実行ではエラーは出ないが無視される
    // 予測（mlPrediction）は後述するpredictAsyncの明示的な呼び出しが必要
    ml.addRow([
      horse.history.timeDeviationValue / 150,
      horse.history.a3hTimeDeviationValue / 150,
      horse.history.runningStyle / 4,
    ], horse.place <= 3 ? 1 : 0);
  }

  // addRowしたデータを使い、結果を予測する
  // mlTrainingの場合、これは無視されて常に空の配列が返される
  const arr = await ml.predictAsync();

  // 空の配列であればプログラムを終了する
  // mlTrainingの場合、ここで終わり
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
  suggestion.quinella(1,
    [ranking[0].horse.number, ranking[1].horse.number],
    [ranking[0].horse.number, ranking[1].horse.number, ranking[2].horse.number]);

  return JSON.stringify(arr);
});


(function () {

  // 学習に名前をつける
  keras.name = 'hello';

  // 学習の設定
  keras.optimizer = 'sgd';
  keras.loss = 'binary_crossentropy';

  // モデルを作るときの設定
  keras.epochs = 10;
  keras.batchSize = 2;
  keras.verbose = 1;

  // 前回学習したモデルにさらにデータを追加する場合はこれのコメントを外す
  //keras.isContinuous = true;

  // 回帰分析をしたい場合はこれのコメントを外す
  //keras.type = 'reguressor';

  // 決定木が欲しい場合はこれのコメントを外し、適宜ラベルをつけておく
  // ファイルはAppData/Local/KMYsofts/KMYKeiba/mlフォルダ内に作られる
  //keras.dotFileName = 'tree.dot';
  //keras.setLabels(JSON.stringify(['number', 'weight']));

  // レイヤー。最初に設定するレイヤーはdense、activation、activityRegularizationのみサポート
  // 以下：対応レイヤー一覧
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  keras.layers.dense(32, 'relu');
  keras.layers.dropout(0.2);
  keras.layers.dense(1, 'sigmoid');
});

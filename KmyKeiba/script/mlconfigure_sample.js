
// こうやって複数の設定を管理すれば容易に切り替えられる（ファイル最後にprofileA()という呼び出しがある）
// どの設定がよいか調べるときに切り替えられると便利
const profileA = () => {

  // 学習に名前をつける。学習ファイルの保存先フォルダにつける名前でもある
  keras.name = 'hello';

  // 学習の設定
  keras.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ にくわえて「radam」も指定可能
  keras.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  // モデルを作るときの設定
  keras.epochs = 10;
  keras.batchSize = 2;
  keras.verbose = 1;

  // 前回学習したモデルにさらにデータを追加する場合はこれのコメントを外す
  // コメントを外す条件として、同じ名前でこれまで学習したことのないデータを学習させること
  // また、レイヤーの設定、オプティマイザなどなども絶対に変えないこと
  // コメントを外さない場合、学習ごとにデータはリセットされる
  //keras.isContinuous = true;

  // 回帰分析をしたい場合はこれのコメントを外す
  //keras.type = 'reguressor';

  // 決定木が欲しい場合はこれのコメントを外し、適宜ラベルをつけておく
  // ファイルはAppData/Local/KMYsofts/KMYKeiba/mlフォルダ内に作られる
  //keras.dotFileName = 'tree.dot';
  //keras.setLabels(JSON.stringify(['number', 'weight']));

  // レイヤー。最初に設定するレイヤーはdense、activation、activityRegularization、batchNormalizationのみサポート
  // input_shapeを指定する必要はない（プログラムで自動で指定します）
  // 以下：対応レイヤー一覧
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  //   batchNormalization()
  keras.layers.dense(32, 'relu');  // reluの部分に指定できるもの https://keras.io/ja/activations/
  keras.layers.dropout(0.2);
  keras.layers.dense(1, 'sigmoid');
};

(function () {
  // 今回は設定Aを使用する
  profileA();
});

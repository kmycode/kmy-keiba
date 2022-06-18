
const profileA = (profile) => {

  // 学習に名前をつける。学習ファイルの保存先フォルダにつける名前でもある
  profile.name = 'local_data';

  // 学習の設定
  profile.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ にくわえて「radam」も指定可能
  profile.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  // モデルを作るときの設定
  profile.epochs = 10;
  profile.batchSize = 2;
  profile.verbose = 1;

  // 前回学習したモデルにさらにデータを追加する場合はこれのコメントを外す
  // コメントを外す条件として、同じ名前でこれまで学習したことのないデータを学習させること
  // また、レイヤーの設定、オプティマイザなどなども絶対に変えないこと
  // コメントを外さない場合、学習ごとにデータはリセットされる
  //profile.isContinuous = true;

  // 回帰分析をしたい場合はこれのコメントを外す
  //profile.type = 'reguressor';

  // 決定木が欲しい場合はこれのコメントを外し、適宜ラベルをつけておく
  // ファイルはAppData/Local/KMYsofts/KMYKeiba/mlフォルダ内に作られる
  //profile.dotFileName = 'tree.dot';
  //profile.setLabels(JSON.stringify(['number', 'weight']));

  // レイヤー。最初に設定するレイヤーはdense、activation、activityRegularization、batchNormalizationのみサポート
  // input_shapeを指定する必要はない（プログラムで自動で指定します）
  // 以下：対応レイヤー一覧
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  //   batchNormalization()
  profile.layers.dense(32, 'relu');  // reluの部分に指定できるもの https://keras.io/ja/activations/
  profile.layers.dropout(0.2);
  profile.layers.dense(1, 'sigmoid');
};

const profileB = (profile) => {

  profile.name = 'central_data';
  profile.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ にくわえて「radam」も指定可能
  profile.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  profile.epochs = 10;
  profile.batchSize = 2;
  profile.verbose = 1;

  profile.layers.dense(32, 'relu');  // reluの部分に指定できるもの https://keras.io/ja/activations/
  profile.layers.dropout(0.2);
  profile.layers.dense(1, 'sigmoid');
};

(function () {
  // 「local」「central」というプロファイルで設定する
  // index.jsからDLを呼び出す時、プロファイル名を指定することで異なる設定を呼び出せる
  profileA(keras.createProfile('local'));
  profileB(keras.createProfile('central'));
});

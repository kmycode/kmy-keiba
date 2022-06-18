
const profileA = (profile) => {

  // �w�K�ɖ��O������B�w�K�t�@�C���̕ۑ���t�H���_�ɂ��閼�O�ł�����
  profile.name = 'local_data';

  // �w�K�̐ݒ�
  profile.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ �ɂ��킦�āuradam�v���w��\
  profile.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  // ���f�������Ƃ��̐ݒ�
  profile.epochs = 10;
  profile.batchSize = 2;
  profile.verbose = 1;

  // �O��w�K�������f���ɂ���Ƀf�[�^��ǉ�����ꍇ�͂���̃R�����g���O��
  // �R�����g���O�������Ƃ��āA�������O�ł���܂Ŋw�K�������Ƃ̂Ȃ��f�[�^���w�K�����邱��
  // �܂��A���C���[�̐ݒ�A�I�v�e�B�}�C�U�ȂǂȂǂ���΂ɕς��Ȃ�����
  // �R�����g���O���Ȃ��ꍇ�A�w�K���ƂɃf�[�^�̓��Z�b�g�����
  //profile.isContinuous = true;

  // ��A���͂��������ꍇ�͂���̃R�����g���O��
  //profile.type = 'reguressor';

  // ����؂��~�����ꍇ�͂���̃R�����g���O���A�K�X���x�������Ă���
  // �t�@�C����AppData/Local/KMYsofts/KMYKeiba/ml�t�H���_���ɍ����
  //profile.dotFileName = 'tree.dot';
  //profile.setLabels(JSON.stringify(['number', 'weight']));

  // ���C���[�B�ŏ��ɐݒ肷�郌�C���[��dense�Aactivation�AactivityRegularization�AbatchNormalization�̂݃T�|�[�g
  // input_shape���w�肷��K�v�͂Ȃ��i�v���O�����Ŏ����Ŏw�肵�܂��j
  // �ȉ��F�Ή����C���[�ꗗ
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  //   batchNormalization()
  profile.layers.dense(32, 'relu');  // relu�̕����Ɏw��ł������ https://keras.io/ja/activations/
  profile.layers.dropout(0.2);
  profile.layers.dense(1, 'sigmoid');
};

const profileB = (profile) => {

  profile.name = 'central_data';
  profile.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ �ɂ��킦�āuradam�v���w��\
  profile.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  profile.epochs = 10;
  profile.batchSize = 2;
  profile.verbose = 1;

  profile.layers.dense(32, 'relu');  // relu�̕����Ɏw��ł������ https://keras.io/ja/activations/
  profile.layers.dropout(0.2);
  profile.layers.dense(1, 'sigmoid');
};

(function () {
  // �ulocal�v�ucentral�v�Ƃ����v���t�@�C���Őݒ肷��
  // index.js����DL���Ăяo�����A�v���t�@�C�������w�肷�邱�ƂňقȂ�ݒ���Ăяo����
  profileA(keras.createProfile('local'));
  profileB(keras.createProfile('central'));
});


// ��������ĕ����̐ݒ���Ǘ�����Ηe�Ղɐ؂�ւ�����i�t�@�C���Ō��profileA()�Ƃ����Ăяo��������j
// �ǂ̐ݒ肪�悢�����ׂ�Ƃ��ɐ؂�ւ�����ƕ֗�
const profileA = () => {

  // �w�K�ɖ��O������B�w�K�t�@�C���̕ۑ���t�H���_�ɂ��閼�O�ł�����
  keras.name = 'hello';

  // �w�K�̐ݒ�
  keras.optimizer = 'sgd';   // https://keras.io/ja/optimizers/ �ɂ��킦�āuradam�v���w��\
  keras.loss = 'binary_crossentropy';  // https://keras.io/ja/losses/

  // ���f�������Ƃ��̐ݒ�
  keras.epochs = 10;
  keras.batchSize = 2;
  keras.verbose = 1;

  // �O��w�K�������f���ɂ���Ƀf�[�^��ǉ�����ꍇ�͂���̃R�����g���O��
  // �R�����g���O�������Ƃ��āA�������O�ł���܂Ŋw�K�������Ƃ̂Ȃ��f�[�^���w�K�����邱��
  // �܂��A���C���[�̐ݒ�A�I�v�e�B�}�C�U�ȂǂȂǂ���΂ɕς��Ȃ�����
  // �R�����g���O���Ȃ��ꍇ�A�w�K���ƂɃf�[�^�̓��Z�b�g�����
  //keras.isContinuous = true;

  // ��A���͂��������ꍇ�͂���̃R�����g���O��
  //keras.type = 'reguressor';

  // ����؂��~�����ꍇ�͂���̃R�����g���O���A�K�X���x�������Ă���
  // �t�@�C����AppData/Local/KMYsofts/KMYKeiba/ml�t�H���_���ɍ����
  //keras.dotFileName = 'tree.dot';
  //keras.setLabels(JSON.stringify(['number', 'weight']));

  // ���C���[�B�ŏ��ɐݒ肷�郌�C���[��dense�Aactivation�AactivityRegularization�AbatchNormalization�̂݃T�|�[�g
  // input_shape���w�肷��K�v�͂Ȃ��i�v���O�����Ŏ����Ŏw�肵�܂��j
  // �ȉ��F�Ή����C���[�ꗗ
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  //   batchNormalization()
  keras.layers.dense(32, 'relu');  // relu�̕����Ɏw��ł������ https://keras.io/ja/activations/
  keras.layers.dropout(0.2);
  keras.layers.dense(1, 'sigmoid');
};

(function () {
  // ����͐ݒ�A���g�p����
  profileA();
});

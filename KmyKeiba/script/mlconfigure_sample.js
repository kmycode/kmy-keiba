
(function () {

  // �w�K�ɖ��O������
  keras.name = 'hello';

  // �w�K�̐ݒ�
  keras.optimizer = 'sgd';
  keras.loss = 'binary_crossentropy';

  // ���f�������Ƃ��̐ݒ�
  keras.epochs = 10;
  keras.batchSize = 2;
  keras.verbose = 1;

  // �O��w�K�������f���ɂ���Ƀf�[�^��ǉ�����ꍇ�͂���̃R�����g���O��
  //keras.isContinuous = true;

  // ��A���͂��������ꍇ�͂���̃R�����g���O��
  //keras.type = 'reguressor';

  // ����؂��~�����ꍇ�͂���̃R�����g���O���A�K�X���x�������Ă���
  // �t�@�C����AppData/Local/KMYsofts/KMYKeiba/ml�t�H���_���ɍ����
  //keras.dotFileName = 'tree.dot';
  //keras.setLabels(JSON.stringify(['number', 'weight']));

  // ���C���[�B�ŏ��ɐݒ肷�郌�C���[��dense�Aactivation�AactivityRegularization�̂݃T�|�[�g
  // �ȉ��F�Ή����C���[�ꗗ
  //   activation(activation)
  //   dropout(rate, seed = null)
  //   flatten(format)
  //   activityRegularization(l1, l2)
  //   masking(value)
  keras.layers.dense(32, 'relu');
  keras.layers.dropout(0.2);
  keras.layers.dense(1, 'sigmoid');
});

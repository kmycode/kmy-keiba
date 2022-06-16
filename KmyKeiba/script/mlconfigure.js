
(function() {
  keras.name = 'hello';
  keras.dense(32, 'relu');
  keras.dense(64, 'relu');
  keras.dense(1, 'sigmoid');
});

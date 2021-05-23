# KMY���n

Windows�œ��삷��A�v���ł��B

![�T���v��](Assets/main.png)

## ����ɕK�v�Ȃ���

���݃o�C�i���͔z�z���Ă��Ȃ��̂ŁA�r���h���Ȃ��ƋN���ł��Ȃ���Ԃł��B
IT�Ɋւ�����m���̂Ȃ����̂����p�͂��T�����������B

### �f�[�^�x�[�X

����ɂ̓f�[�^�x�[�X�T�[�o�[���K�v�ł��B`ROW_NUMBER`���g����o�[�W�����łȂ���΂����܂���B
* MariaDB 10.2 �ȍ~
* MySQL 8 �ȍ~

�C���X�g�[������root�̃p�X���[�h����͂����ʂ��o��Ǝv���̂ŁA���̃p�X���[�h��Y�ꂸ�Ƀ������Ă��������B

### ���p�L�[�i�L���j

���n�f�[�^�̎擾�ɂ́A�ʓr�L���_�񂪕K�v�ł��B�i�����{�n���̏ꍇ�A������4000�~�j  
���L�̕Е��܂��͗������_�񂵂Ȃ��ƁA���n�f�[�^���擾�ł��܂���B

* [JRA-VAN�f�[�^���{���](https://jra-van.jp/dlb/)
  * �������n�̃f�[�^�擾�ɕK�v�ł�
  * �_���A���p�L�[���擾���Ă��������B�uJRA���[�V���O�r���A�[�v�͍��̂Ƃ���s�v�ł�
* [�n�����nDATA](https://saikyo.k-ba.com/members/chihou/)
  * �n�����n�̃f�[�^�擾�ɕK�v�ł�
  * �_���A���p�L�[���擾���Ă��������B���p�L�[�ȊO�ɂ������̍w���I�v�V����������܂����S�ĕs�v�ł�

���p�L�[�͏����ݒ莞�ɐݒ肵�܂��B

### �f�[�^�擾�̂��߂̃\�t�g

�N�����̂��̂ɂ͕s�v�ł����A�r���h�̎���***����***�K�v�ɂȂ�܂��B

* [JV-Link](https://jra-van.jp/dlb/) - �u������iJV-Link�j�v�^�u���_�E�����[�h�ł��܂�
* [UmaConn](https://saikyo.k-ba.com/members/chihou/) - �_�E�����[�h�{�^�����_�E�����[�h�ł��܂�

���p�L�[��ݒ肷��K�v������܂����A�r���h���ăA�v�����N��������ł��ݒ�ł��܂��B

### �����ݒ�

`KmyKeiba` �v���W�F�N�g�� `database.txt` �Ƃ����t�@�C��������܂��B

```
host=localhost
database=kmykeiba
username=root
password=takaki
```

�����̐ݒ��ύX���ĕۑ����Ă��������B������ `username` �́A�f�[�^�x�[�X�쐬�i`CREATE TABLE`�j���������������̂�ݒ肵�Ă��������B

�܂��A�A�v�����N��������A�u�t�@�C���v���j���[���JV-Link��UmaConn�̐ݒ��ʂ��J���āA���p�L�[��ݒ肵�Ă��������B

### ���n�f�[�^�̎擾

�u�t�@�C���v���j���[���A�ŐV�f�[�^���擾���܂��B�_�C�A���O�����ƁA�擾�����f�[�^����ʂɔ��f����܂��B

## �r���h

.NET 5.0��C# 9�ŊJ�����Ă��邽�߁AVisual Studio 2019�ȍ~���K�v�ł��B�C���X�g�[�����ɁA�f�X�N�g�b�v�A�v���Ƀ`�F�b�N�����Ă��������B

### �r���h�ɕK�v�Ȃ���

�r���h�ɂ́A��L�uJV-Link�v�uUmaConn�v�̗������K�v�ł��B

�܂��A���̃��|�W�g���ł́A���쌠�̊֌W�Ō������Ă���t�@�C�������݂��Ă���A���ꂪ�Ȃ��ƃr���h�ł��܂���B  
[Data Lab. SDK](https://jra-van.jp/dlb/sdv/sdk.html)���SDK�{�̂��_�E�����[�h���Ă��������BVer.4.6.0�ł́A�ȉ��̂悤�ȍ\���ɂȂ��Ă��܂��B

```
JV-Data�\����
JV-Link
�T���v���v���O����
�h�L�������g
```

���̂����uJV-Data�\���́v�t�H���_�̒��́uC#�Łv�Ɋ܂܂�� `JVData_Struct.cs` �t�@�C���� `structures.cs` �Ƀ��l�[���̂����A�ȉ��̃f�B���N�g���ɃR�s�[���Ă��������B  
`JVLib` �t�H���_���Ȃ��ꍇ�͍쐬���Ă��������B

```
KmyKeiba.JVLink/JVLib/structures.cs
```

����ɁA `structures.cs` ���ȉ��̂悤�ɕҏW���Ă��������B

```c#
using System.Text;

#nullable disable

namespace KmyKeiba.JVLink.Wrappers.JVLib
{

    <�����Ɍ��X��structures.cs�̓��e��}��>

}
```

### `Add-Migration` �ɂ���

���̃v���O������EntityFrameworkCore���g�p���Ă��܂��B`Add-Migration` �����s����Ƃ��ɂ́A�v���W�F�N�g�̃\�����[�V�����v���b�g�t�H�[���� `Any CPU` �ɂ��Ă��������B�łȂ��ƃG���[�ɂȂ�܂��B

�������A�A�v�������s���鎞�ɂ͕K�� `x86` �ɖ߂��Ă��������B���x��JV-Link��UmaConn��COM���ǂݍ��܂�Ȃ��Ȃ�܂��B

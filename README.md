# KMY���n

Windows�œ��삷�鋣�n���{���p�̃A�v���ł��BJRA-VAN�A�n�����nDATA�̃f�[�^���g�p���܂��B

## �v���W�F�N�g

| �v���W�F�N�g�� | ���� |
| --- | --- |
| `KmyKeiba` | ���C���A�v���B64bit�ł̃r���h��z�肵�Ă��܂� |
| `KmyKeiba.Downloader` | ���n�f�[�^���_�E�����[�h����A�v���B<br>����̓n�[�h�R�[�f�B���O�ł��B�v���O�����R�[�h�����������Ȃ���g���Ă܂��B<br>��q�������ȏꍇ�������A32bit�Ńr���h���Ă������� |
| `KmyKeiba.JVLink` | JV-LINK�A�n�����nDATA���f�[�^���_�E�����[�h���邽�߂̃v���O�����ł� |
| `KmyKeiba.Data` | ���n�f�[�^�̌^��`���܂܂�܂��B�����ɂ���`MyContextBase`���p�������N���X�𗘗p���邱�ƂŁA�f�[�^�x�[�X�փA�N�Z�X�ł��܂� |
| ����ȊO | ���̓����e�i���X���Ă��܂���B`Keras.NET`���g�p�����f�B�[�v���[�j���O�Ƃ��Ƃ��̃R�[�h�������Ă܂��B�~�����l�͓K���Ɏ����Ă��Ă��������B�T�|�[�g�͂��܂��� |

## ����ɕK�v�Ȃ���

���݃o�C�i���͔z�z���Ă��Ȃ��̂ŁA�r���h���Ȃ��ƋN���ł��Ȃ���Ԃł��B
�܂��AMariaDB�Ƃ����f�[�^�x�[�X�\�t�g��ʓr��������K�v������܂����A����͒ʏ�͋Z�p�҂��Z�b�g�A�b�v����A��舵���ɐ��m����v������\�t�g�E�F�A�ł��B�i�C���X�g�[�����͓̂���Ȃ��̂ŁA������菇�����쐬���邩������܂���j

IT�Ɋւ�����m���̂Ȃ����̂����p�͂��T�����������B

### �f�[�^�x�[�X

����ɂ̓f�[�^�x�[�X�T�[�o�[���K�v�ł��B`ROW_NUMBER`���g����o�[�W�����łȂ���΂����܂���B
* MariaDB 10.2 �ȍ~
* MySQL 8 �ȍ~

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

* [JV-Link](https://jra-van.jp/dlb/) - �u������iJV-Link�j�v�^�u���_�E�����[�h�ł��܂�
* [UmaConn](https://saikyo.k-ba.com/members/chihou/) - �_�E�����[�h�{�^�����_�E�����[�h�ł��܂�

����������p�L�[��ݒ肷��K�v������܂��B

### �����ݒ�i���\��j

���̂悤�Ȑݒ肪�K�v�ɂȂ�\��ł��B�i���̓v���O�������̃R�[�h�𒼐ڏ��������Ă��������j

`KmyKeiba` �v���W�F�N�g�� `database.txt` �Ƃ����t�@�C��������܂��B

```
host=localhost
database=kmykeiba
username=root
password=takaki
```

�����̐ݒ��ύX���ĕۑ����Ă��������B������ `username` �́A�f�[�^�x�[�X�쐬�i`CREATE TABLE`�j���������������̂�ݒ肵�Ă��������B

## �r���h

.NET 6.0��C# 10�ŊJ�����Ă��邽�߁AVisual Studio 2022�ȍ~���K�v�ł��B�C���X�g�[�����ɁA�f�X�N�g�b�v�A�v���Ƀ`�F�b�N�����Ă��������B

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

    // <�����Ɍ��X��structures.cs�̓��e��}��>

    // <JVData_Struct�̍ŏ��̍s��partial��ǉ����Ă�������>
    // public static partial class JVData_Struct

}
```

### `Add-Migration` �ɂ���

���̃v���O������EntityFrameworkCore���g�p���Ă��܂��B`Add-Migration` �����s����Ƃ��ɂ́A�v���W�F�N�g�̃\�����[�V�����v���b�g�t�H�[���� `Any CPU` �܂��� `x64` �ɂ��Ă��������B�łȂ��ƃG���[�ɂȂ�܂��B

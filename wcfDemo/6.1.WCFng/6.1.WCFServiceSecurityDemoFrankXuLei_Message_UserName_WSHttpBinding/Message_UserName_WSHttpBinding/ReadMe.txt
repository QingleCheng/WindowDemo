1.//Coded By Frank Xu Lei 8/24/2009.��WCF�ֲ�ʽ��ȫ����ʵ����8ʾ������:
��Ϣ��ȫģʽ��UserName��WSHttpBinding��
�Զ����й��������ù��̰����������ֵ����ã�
1��һ���������ࡱWCFService��������Լ�Ͳ�����Լ,�������ʹ��������Լ��
2��һ��������������WCFHost������һ��Ӧ�ó�����ͽ��̣������ڸû��������С� 
3��һ�����ս�㡱Client���ɿͻ������ڷ��ʷ���
// 
2.//Message_UserName_WSHttpBinding
��ȫģʽ: ����
 
��������: �����е� Web ����ͻ��˺ͷ�����л�����
 
�����֤����������:�ǣ�ʹ�� HTTP��

�����֤���ͻ��ˣ�:�ǣ�UserName Password��
 
������: ��
 
������: ��
 
Transport: HTTPS
 
��: WSHttpBinding
3.//Marks
��������Ҫһ����Ч�Ŀ����ڰ�ȫ�׽��ֲ� (SSL) �� X.509 ֤�� 
4.//makecert 
makecert -sr CurrentUser -ss My -n CN=FrankCertificate -sky signature -pe
5.Set certificate trusted.
http://www.cnblogs.com/frank_xl/archive/2009/07/30/1534624.html
http://www.cnblogs.com/frank_xl/archive/2009/03/01/1400751.html
6.
������Makecert.exe��������֤�飬�ɽ�����Կ����Ϣ��ȫ������ʹ�ã����Ե�����Կ��
ʹ����������

makecert -sr localmachine -ss My -n CN=WCFServer -sky exchange -pe -r

���Ƿ����֤�顣

makecert -sr currentuser -ss My -n CN=WCFClient -sky exchange -pe -r

���ǿͻ���֤�顣
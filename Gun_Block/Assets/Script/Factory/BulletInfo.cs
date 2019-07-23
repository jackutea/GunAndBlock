using System;
using System.Text;
using System.Security.Cryptography;


[Serializable]
public class BulletInfo {

    public string bid;
    public string sid;
    public float shootSpeed;
    public sbyte direct;
    public float dmg;

    public BulletInfo(string sid) {

        this.sid = sid;

        this.bid = getBulletId(this.sid);

    }

    string getBulletId(string sid) {

        string timeStr = DateTime.Now.Ticks.ToString();

        MD5 md5 = new MD5CryptoServiceProvider();

        byte[] s = Encoding.UTF8.GetBytes(timeStr + sid);

        byte[] c = md5.ComputeHash(s);

        string bidStr = Convert.ToBase64String(c);

        return bidStr;

    }

    public void bePerfectBlock() {

        this.direct *= -1;

        this.shootSpeed *= 1.2f;

        this.dmg *= 1.2f;
    }

}
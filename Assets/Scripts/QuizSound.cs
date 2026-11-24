using UnityEngine;

public class QuizSound : MonoBehaviour {
    AudioSource source;
    AudioClip tap,win,miss;
    void Awake() {
        source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;
        tap=Tone(420,.07f);win=Tone(740,.16f);miss=Tone(175,.19f);
    }
    static AudioClip Tone(float hz,float length) {
        const int rate=22050;int n=(int)(rate*length);var wave=new float[n];
        for(int i=0;i<n;i++)wave[i]=.22f*Mathf.Sin(2*Mathf.PI*hz*i/rate)*Mathf.Min(1,i/60f)*(1-i/(float)n);
        var clip=AudioClip.Create("Pulso "+hz,n,1,rate,false);clip.SetData(wave,0);return clip;
    }
    public void Play(int type=0) {source.PlayOneShot(type==1?win:type==2?miss:tap,PlayerPrefs.GetInt("volume",70)/100f);}
    void OnDestroy(){Destroy(tap);Destroy(win);Destroy(miss);}
}

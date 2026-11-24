using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class ProjecteKahoot : MonoBehaviour {
    static ProjecteKahoot active;
    QuizStore store;
    QuizView view;
    QuizSound audioFx;
    Quiz quiz,boardQuiz;
    string player="",notice="",logPath="";
    int index,points,hits,lang;
    double start,elapsed;
    bool answered,saved;
    Text timer,feedback;
    readonly List<Button> choices=new List<Button>();
    string Scene => SceneManager.GetActiveScene().name;
    string T(string es,string ca,string en)=>lang==1?ca:lang==2?en:es;
    void Awake() {
        if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);
        store=new QuizStore();lang=PlayerPrefs.GetInt("lang",0);player=PlayerPrefs.GetString("player",T("Jugador","Jugador","Player"));
        var camera=gameObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=QuizView.Hex("#152038");camera.cullingMask=0;
        gameObject.AddComponent<AudioListener>();audioFx=gameObject.AddComponent<QuizSound>();

        SceneManager.sceneLoaded+=Loaded;
    }
    void OnDestroy(){if(active==this){SceneManager.sceneLoaded-=Loaded;active=null;}}
    void Loaded(Scene scene,LoadSceneMode mode){Draw();}
    void Start(){if(active==this && view==null)Draw();}
    void Go(string name){notice="";audioFx.Play();SceneManager.LoadScene(name);}
    void Page(string title) {

        view=new QuizView(title);
    }
    void HomeButton(){view.Button(T("Menú principal","Menú principal","Main menu"),()=>Go("MenuPrincipal"));}
    void Draw() {
        switch(Scene) {
        case "SelectorCuestionarios":SelectQuiz();break;
        case "CrearCuestionario":EditorPage();break;
        case "Juego":if(quiz==null){SelectQuiz();break;}GamePage();break;
        case "Resultados":if(quiz==null){SelectQuiz();break;}Results();break;
        case "ListaCuestionarios":Rankings();break;
        case "Informes":ReportsPage();break;
        case "Ajustes":Settings();break;
        case "AcercaDe":About();break;
        default:Home();break;
        }
        view.Finish();view.Focus();
    }
    public static void FromMenu(string sceneName) {
        if (active == null) { SceneManager.LoadScene(sceneName); return; }
        if (sceneName == "ListaCuestionarios") active.boardQuiz = null;
        if (sceneName == "Informes") active.logPath = "";
        active.Go(sceneName);
    }
    void Home(){
        Page("Kahoot");
        view.Button(T("Jugar","Jugar","Play"),()=>Go("SelectorCuestionarios"),view.tiles[0]);
        view.Button(T("Crear cuestionario","Crear qüestionari","Create a quiz"),()=>Go("CrearCuestionario"),view.tiles[1]);
        view.Button(T("Clasificaciones","Classificacions","Leaderboards"),()=>{boardQuiz=null;Go("ListaCuestionarios");},view.tiles[2]);
        view.Button(T("Ajustes","Configuració","Settings"),()=>Go("Ajustes"));
        view.Button(T("Informes","Informes","Reports"),()=>{logPath="";Go("Informes");});
        view.Button(T("Acerca de","Quant a","About"),()=>Go("AcercaDe"));
    }
    void SelectQuiz(){
        Page(T("Elige un cuestionario","Tria un qüestionari","Choose a quiz"));
        var name=view.Field(T("Nombre del jugador","Nom del jugador","Player name"),player,s=>player=s);
        var msg=view.Label(notice,16);
        foreach(var q in store.Catalog())view.Button(q.title+"   /   "+q.questions.Length,()=>{
            if(string.IsNullOrWhiteSpace(player)){msg.text=T("Escribe tu nombre.","Escriu el teu nom.","Enter your name.");view.Focus(name);return;}
            player=player.Trim();PlayerPrefs.SetString("player",player);PlayerPrefs.Save();quiz=q;index=0;points=0;elapsed=0;hits=0;Go("Juego");
        });
        HomeButton();
    }
    void GamePage(){
        var p=quiz.questions[index];Page(quiz.title);
        timer=view.Label("");view.Label(p.prompt,28);choices.Clear();answered=false;
        RectTransform row=null;
        for(int i=0;i<p.answers.Length;i++){
            if(i%2==0)row=view.Row("Opciones");
            int choice=i;choices.Add(view.Button((i+1).ToString("00")+"   "+p.answers[i],()=>Answer(choice),view.tiles[i%4],true,row,92));
        }
        for(int i=0;i<choices.Count;i++){choices[i].image.color=view.tiles[i%4];choices[i].GetComponentInChildren<Text>().color=Color.white;}
        feedback=view.Label("",21);start=Time.unscaledTimeAsDouble;Tick();
    }
    void Update(){
        if(active!=this||view==null||view.canvas==null)return;
        view.Navigation();if(Scene=="Juego"&&quiz!=null&&!answered){
            Tick();
            for(int i=0;i<Mathf.Min(9,choices.Count)&&!answered;i++)
                if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))||Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad1+i)))Answer(i);
        }
    }
    void Tick(){
        double remaining=Math.Max(0,quiz.questions[index].seconds-(Time.unscaledTimeAsDouble-start));
        timer.text=(index+1)+" / "+quiz.questions.Length+"     ·     "+T("Tiempo","Temps","Time")+"   "+Math.Ceiling(remaining)+" s     /     "+points+" "+T("puntos","punts","points");
        if(view.meter!=null){
            float fraction=(float)(remaining/quiz.questions[index].seconds);
            view.meter.anchorMax=new Vector2(Mathf.Clamp01(fraction),1);
            view.meter.GetComponent<Image>().color=fraction<.25f?QuizView.Hex("#FF996E"):view.mint;
        }
        if(remaining<=0)Answer(-1);
    }
    void Answer(int choice){
        if(answered)return;answered=true;var p=quiz.questions[index];
        double used=Math.Min(p.seconds,Math.Max(0,Time.unscaledTimeAsDouble-start));elapsed+=used;
        if(used>=p.seconds)choice=-1;
        bool right=choice==p.correct-1;
        if(right){hits++;points+=100+Mathf.RoundToInt(900*(1-(float)(used/p.seconds)));}
        foreach(var b in choices)b.interactable=false;
        choices[p.correct-1].image.color=view.mint;
        choices[p.correct-1].GetComponentInChildren<Text>().color=view.ink;
        audioFx.Play(right?1:2);
        timer.text=points+" "+T("puntos","punts","points");
        feedback.text=(right?T("¡Acierto!","Encert!","Correct!"):choice<0?T("Tiempo agotado.","Temps esgotat.","Time is up."):T("Incorrecta.","Incorrecta.","Incorrect."))+"   "+T("Respuesta correcta","Resposta correcta","Correct answer")+" · "+p.answers[p.correct-1];
        var next=view.Button(index+1==quiz.questions.Length?T("Ver resultados","Veure resultats","See results"):T("Siguiente pregunta","Pregunta següent","Next question"),()=>{
            if(index+1<quiz.questions.Length){index++;Draw();return;}
            saved=store.Record(quiz,new Score{player=player,points=points,seconds=elapsed});Go("Resultados");
        });view.Focus(next);
    }
    void Results(){
        Page(T("Resultado","Resultat","Result"));
        view.Label(player+"   /   "+points+" "+T("puntos","punts","points"),30);
        view.Label(hits+" / "+quiz.questions.Length+"   ·   "+elapsed.ToString("0.00")+" s");
        if(saved){
            int rank=1+store.Ranking(quiz).Count(s=>s.points>points || (s.points==points && s.seconds<elapsed));
            view.Label(T("Posición","Posició","Rank")+"   #"+rank);
        }else view.Label(T("Error al guardar el XML","Error en desar l’XML","Could not save XML")+" · "+store.LastError);
        view.Button(T("Ver clasificación","Veure classificació","View leaderboard"),()=>{boardQuiz=quiz;Go("ListaCuestionarios");});
        view.Button(T("Otra partida","Una altra partida","Play again"),()=>Go("SelectorCuestionarios"));HomeButton();
    }
    void Rankings(){
        Page(T("Clasificaciones","Classificacions","Leaderboards"));
        if(boardQuiz==null){
            foreach(var q in store.Catalog())view.Button(q.title,()=>{boardQuiz=q;Draw();});
        }else{
            var scores=store.Ranking(boardQuiz);if(scores.Count==0)view.Label(T("Sin puntuaciones todavía","Encara sense puntuacions","No scores yet"));
            int rank=0;foreach(var s in scores)view.Label((++rank)+".   "+s.player+"     "+s.points+" pts    "+s.seconds.ToString("0.00")+" s");
            view.Button(T("Todos los cuestionarios","Tots els qüestionaris","All quizzes"),()=>{boardQuiz=null;Draw();});
        }HomeButton();
    }
    void ReportsPage(){
        Page(T("Informes","Informes","Reports"));
        if(string.IsNullOrEmpty(logPath)){
            var files=store.Logs();if(files.Length==0)view.Label(T("Todavía no hay incidencias.","Encara no hi ha incidències.","No incidents recorded."));
            foreach(var f in files)view.Button(Path.GetFileName(f),()=>{logPath=f;Draw();});
        }else{
            view.Label(store.ReadLog(logPath),17);view.Button(T("Volver a informes","Tornar als informes","Back to reports"),()=>{logPath="";Draw();});
        }HomeButton();
    }
    void Settings(){
        Page(T("Ajustes","Configuració","Settings"));
        view.Label(T("Idioma de la interfaz","Idioma de la interfície","Interface language"));
        string[] languages={"Español","Català","English"};
        for(int i=0;i<languages.Length;i++){int n=i;view.Button((lang==i?"●   ":"")+languages[i],()=>{lang=n;PlayerPrefs.SetInt("lang",n);PlayerPrefs.Save();Draw();});}
        int v=PlayerPrefs.GetInt("volume",70);view.Label(T("Volumen de efectos","Volum dels efectes","Effects volume")+"   "+v+"%");
        view.Button("− 10%",()=>Volume(v-10),null,v>0);view.Button("+ 10%",()=>Volume(v+10),null,v<100);
        view.Button(T("Probar sonido","Provar el so","Test sound"),()=>audioFx.Play(1));HomeButton();
    }
    void Volume(int v){PlayerPrefs.SetInt("volume",Mathf.Clamp(v,0,100));PlayerPrefs.Save();audioFx.Play();Draw();}
    void About(){
        Page("ProjecteKahoot");
        view.Label(T("Concurso de preguntas por tiempo. Cada acierto vale de 100 a 1000 puntos según la rapidez.",
        "Concurs de preguntes amb temps. Cada encert val de 100 a 1000 punts segons la rapidesa.",
        "Timed quiz. Each correct answer earns 100–1000 points depending on speed."));
        view.Label(T("Flechas o cruceta para moverte. Intro o botón A para elegir.","Fletxes o creueta per moure’t. Retorn o botó A per triar.","Use arrows or the D-pad to move. Enter or A selects."));
        HomeButton();
    }
}

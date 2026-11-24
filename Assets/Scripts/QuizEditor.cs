using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class ProjecteKahoot {
    class Draft {
        public string prompt="",seconds="20";
        public List<string> answers=new List<string>{"",""};
        public int correct=1;
    }
    string title="";
    readonly List<Draft> drafts=new List<Draft>{new Draft()};
    void RefreshAt(float pos){
        int selected=view.SelectionIndex();Draw();view.FocusIndex(selected);Canvas.ForceUpdateCanvases();view.scroll.verticalNormalizedPosition=pos;
    }
    void EditorPage(){
        Page(T("Crear cuestionario","Crear qüestionari","Create a quiz"));
        view.Field(T("Nombre del cuestionario","Nom del qüestionari","Quiz title"),title,s=>title=s);
        view.Label(T("Marca una respuesta correcta por pregunta.","Marca una resposta correcta per pregunta.","Mark one correct answer per question."),17);
        for(int i=0;i<drafts.Count;i++){
            var d=drafts[i];
            view.Field(T("Pregunta","Pregunta","Question")+" "+(i+1),d.prompt,s=>d.prompt=s);
            view.Field(T("Duración en segundos (1–600)","Durada en segons (1–600)","Time in seconds (1–600)"),d.seconds,s=>d.seconds=s,true);
            for(int a=0;a<d.answers.Count;a++){
                int n=a;view.Field(T("Respuesta","Resposta","Answer")+" "+(a+1),d.answers[a],s=>d.answers[n]=s);
                view.Button((d.correct==n+1?"●   ":"○   ")+T("Correcta","Correcta","Correct")+" · "+(n+1),()=>{d.correct=n+1;RefreshAt(view.scroll.verticalNormalizedPosition);},d.correct==n+1?view.tiles[2]:view.panel);
            }
            var responseTools=view.Row("BotonesRespuesta");
            view.Button(T("+ Respuesta","+ Resposta","+ Answer"),()=>{d.answers.Add("");RefreshAt(view.scroll.verticalNormalizedPosition);},null,true,responseTools);
            view.Button(T("− Respuesta","− Resposta","− Answer"),()=>{d.answers.RemoveAt(d.answers.Count-1);d.correct=Mathf.Min(d.correct,d.answers.Count);RefreshAt(view.scroll.verticalNormalizedPosition);},null,d.answers.Count>2,responseTools);
        }
        var questionTools=view.Row("BotonesPregunta");
        view.Button(T("+ Pregunta","+ Pregunta","+ Question"),()=>{drafts.Add(new Draft());RefreshAt(0);},view.tiles[0],true,questionTools);
        view.Button(T("− Pregunta","− Pregunta","− Question"),()=>{drafts.RemoveAt(drafts.Count-1);RefreshAt(0);},null,drafts.Count>1,questionTools);
        var status=view.Label(notice,18);
        view.Button(T("Guardar y jugar","Desar i jugar","Save and play"),()=>{
            var q=new Quiz{title=title.Trim(),questions=drafts.Select(d=>new Question{prompt=d.prompt.Trim(),answers=d.answers.Select(a=>a.Trim()).ToArray(),correct=d.correct,seconds=int.TryParse(d.seconds,out int s)?s:0}).ToArray()};
            try {QuizStore.Validate(q);} catch(System.Exception){
                status.text=T("Completa el título, los enunciados, todas las respuestas y los tiempos (1–600).","Completa el títol, els enunciats, totes les respostes i els temps (1–600).","Complete the title, questions, answers and times (1–600).");return;
            }
            if(store.Save(q)){title="";drafts.Clear();drafts.Add(new Draft());Go("SelectorCuestionarios");}else status.text=store.LastError;
        },view.tiles[2]);
        view.Button(T("Crear ejemplo de 10 preguntas","Crear exemple de 10 preguntes","Create 10-question example"),()=>{
            var q=JsonUtility.FromJson<Quiz>(Resources.Load<TextAsset>("Quizzes/orbitas").text);
            q.title=T("Ejemplo","Exemple","Example")+" · "+System.DateTime.Now.ToString("HH.mm.ss");
            if(store.Save(q))Go("SelectorCuestionarios");else status.text=store.LastError;
        });
        HomeButton();
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[Serializable] public class Question {
    public string prompt;
    public string[] answers;
    public int correct; // 1-based in the JSON format.
    public int seconds;
}
[Serializable] public class Quiz {
    public string title;
    public Question[] questions;
    [NonSerialized] public string key;
}
public class Score {
    public string player;
    public int points;
    public double seconds;
}
public class Board { public List<Score> entries = new List<Score>(); }

public class QuizStore {
    readonly string root = Application.persistentDataPath;
    public string Reports => Path.Combine(root, "Informes");
    string Quizzes => Path.Combine(root, "Cuestionarios");
    string Boards => Path.Combine(root, "Resultados");
    public QuizStore() {
        bool previous=PlayerPrefs.GetInt("dataReady",0)==1;
        try {
            foreach(var path in new[]{Reports,Quizzes,Boards}){
                if(Directory.Exists(path))continue;
                Directory.CreateDirectory(path);
                if(previous)Log(path,new DirectoryNotFoundException(path));
            }
            PlayerPrefs.SetInt("dataReady",1);PlayerPrefs.Save();
        }catch(Exception e){Log(root,e);}
    }
    public string LastError { get; private set; }
    string Stamp() => DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0,8);

    void Log(string source, Exception ex) {
        LastError = source + " | " + ex.Message;
        Debug.LogWarning(LastError);
        try {
            Directory.CreateDirectory(Reports);
            File.WriteAllText(Path.Combine(Reports, Stamp()+".txt"), DateTime.Now.ToString("O")+"\n"+source+"\n"+ex);
        } catch(Exception io) { Debug.LogWarning(io.Message); }
    }
    public static void Validate(Quiz q) {
        if(q == null || string.IsNullOrWhiteSpace(q.title) || q.questions == null || q.questions.Length == 0)
            throw new InvalidDataException("title / questions");
        foreach(var p in q.questions)
            if(p == null || string.IsNullOrWhiteSpace(p.prompt) || p.answers == null || p.answers.Length < 2 ||
               p.answers.Any(string.IsNullOrWhiteSpace) || p.correct < 1 || p.correct > p.answers.Length || p.seconds < 1 || p.seconds > 600)
                throw new InvalidDataException("prompt / answers / correct / seconds (1–600)");
    }
    void Parse(List<Quiz> list, string key, Func<string> read) {
        try {
            var q = JsonUtility.FromJson<Quiz>(read()); Validate(q); q.key=key; list.Add(q);
        } catch(Exception e) { Log(key,e); }
    }
    public List<Quiz> Catalog() {
        var list=new List<Quiz>();
        foreach(var a in Resources.LoadAll<TextAsset>("Quizzes"))
            Parse(list,"base-"+a.name,()=>a.text);
        try {
            Directory.CreateDirectory(Quizzes);
            foreach(var f in Directory.GetFiles(Quizzes,"*.json"))
                Parse(list,"user-"+Path.GetFileNameWithoutExtension(f),()=>File.ReadAllText(f));
        } catch(Exception e) { Log(Quizzes,e); }
        return list.OrderBy(q=>q.title).ThenBy(q=>q.key).ToList();
    }
    public bool Save(Quiz q) {
        try {
            Validate(q); Directory.CreateDirectory(Quizzes);
            string id=Guid.NewGuid().ToString("N"), path=Path.Combine(Quizzes,id+".json");
            Atomic(path,tmp=>File.WriteAllText(tmp,JsonUtility.ToJson(q,true))); q.key="user-"+id; return true;
        } catch(Exception e) { Log(Quizzes,e); return false; }
    }
    static void Atomic(string path, Action<string> write) {
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try { write(temp); if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path); }
        finally { if(File.Exists(temp)) File.Delete(temp); }
    }
    string BoardPath(Quiz q) => Path.Combine(Boards,q.key+".xml");
    static Board ReadBoard(string path) {
        if(!File.Exists(path)) return new Board();
        using(var f=File.OpenRead(path)) {
            var b=(Board)new XmlSerializer(typeof(Board)).Deserialize(f);
            if(b==null || b.entries==null || b.entries.Any(s=>s==null || string.IsNullOrWhiteSpace(s.player) || s.points<0 || s.seconds<0 || double.IsNaN(s.seconds) || double.IsInfinity(s.seconds))) throw new InvalidDataException("XML");
            return b;
        }
    }
    public List<Score> Ranking(Quiz q) {
        try { return ReadBoard(BoardPath(q)).entries.OrderByDescending(s=>s.points).ThenBy(s=>s.seconds).ToList(); }
        catch(Exception e) { Log(BoardPath(q),e); return new List<Score>(); }
    }
    public bool Record(Quiz q, Score score) {
        try {
            Directory.CreateDirectory(Boards); var path=BoardPath(q); var board=ReadBoard(path);
            board.entries.Add(score);
            Atomic(path,tmp=>{using(var f=File.Create(tmp)) new XmlSerializer(typeof(Board)).Serialize(f,board);}); return true;
        } catch(Exception e) { Log(BoardPath(q),e); return false; }
    }
    public string[] Logs() {
        try { Directory.CreateDirectory(Reports); return Directory.GetFiles(Reports,"*.txt").OrderByDescending(x=>x).ToArray(); }
        catch(Exception e) { Log(Reports,e); return Array.Empty<string>(); }
    }
    public string ReadLog(string path) {
        try {return File.ReadAllText(path);} catch(Exception e) {Log(path,e);return LastError;}
    }
}

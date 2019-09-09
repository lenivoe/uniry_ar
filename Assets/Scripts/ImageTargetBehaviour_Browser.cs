using UnityEngine;

public class ImageTargetBehaviour_Browser : ImageTargetBehaviour {
    public string link;

    
    private MessagerBehaviour messager = null;


    protected override void Start () {
        base.Start();

        TargetFound += OnTargetFound;
        messager = FindObjectOfType<MessagerBehaviour>();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        TargetFound -= OnTargetFound;
    }
    
    void OnTargetFound(EasyAR.TargetAbstractBehaviour obj) {
        if (!AppController.Inst.HaveInetConnection) {
            messager.SetMessege("Проблемы с соединением...");
            return;
        }

        Application.OpenURL(link);
    }
}

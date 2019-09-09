
public class ImageTargetBehaviour_PdfViewer : ImageTargetBehaviour_Downloader {
    protected override void OnResourceReady(string name) {
        PdfViewer.Inst.StartActivityAsync(name);
    }
}

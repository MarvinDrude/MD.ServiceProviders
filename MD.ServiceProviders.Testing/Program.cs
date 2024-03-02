
using MD.ServiceProviders.Container;
using MD.ServiceProviders.Service;

var container = new PreviewGeneratorContainer();
container.SetService(new VideoPreviewGenerator());
container.SetService(new DocumentPreviewGenerator());

//if(container.TryGetService<DocumentPreviewGenerator>(out var generator)) {
//    string w = "";
//}

var previewGenerator = await container.RetrieveService(new PreviewParams() {
    Filename = "file.video"
});

Console.WriteLine("End.");

public sealed class PreviewGeneratorContainer : ServiceContainer<PreviewGenerator, PreviewParams> {

    public override async Task<PreviewGenerator?> RetrieveService(PreviewParams parameters) {

        if(parameters.Filename.EndsWith(".video")) {
            return GetService<VideoPreviewGenerator>();
        }

        if (parameters.Filename.EndsWith(".doc")) {
            return GetService<DocumentPreviewGenerator>();
        }

        return null;

    }

}

public sealed class PreviewParams {

    public required string Filename { get; set; }

}

public interface PreviewGenerator : IServiceImplementation {

    public Task<string> Generate(string filename);

}

public class VideoPreviewGenerator : PreviewGenerator {

    public async Task<string> Generate(string filename) {
        return "video-preview";
    }

}

public class DocumentPreviewGenerator : PreviewGenerator {

    public async Task<string> Generate(string filename) {
        return "document-preview";
    }

}
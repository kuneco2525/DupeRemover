namespace DupeRemover;

internal class Contents {
	internal readonly DateTime Create, LastWrite;
	internal string FullName, Name;
	internal long Size;
	internal bool Dupe;

	internal Contents(FileInfo i) {
		Create = i.CreationTimeUtc;
		LastWrite = i.LastWriteTimeUtc;
		if(Create > LastWrite) { Create = LastWrite; }
		FullName = i.FullName.Replace('\\', '/');
		Name = i.Name;
		Size = i.Length;
	}
}
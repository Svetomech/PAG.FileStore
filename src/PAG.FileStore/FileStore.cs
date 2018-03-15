using Inflector;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PAG.FileStore
{
    // TODO: Proper exception handling in Save() and Load()
    // TODO: Proper validation in StoreDirectory, StoreFileName
    public class FileStore<T> : ICollection<T>
    {
        private List<T> _storeFiles; // Replace with HashSet<T> to disallow duplicates?
        private string _storeDirectory;
        private string _storeFileName;

        public FileStore() : this(default) { }

        public FileStore(string storePath)
        {
            Inflector.Inflector.SetDefaultCultureFunc = () => CultureInfo.GetCultureInfo("en-US");

            _storeFiles = new List<T>();

            if (String.IsNullOrWhiteSpace(storePath))
            {
                _storeDirectory = Directory.GetCurrentDirectory(); // Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                _storeFileName = GetType().GenericTypeArguments[0].Name.Pluralize().ToLowerInvariant() + ".json";
            }
            else
            {
                _storeDirectory = Path.GetDirectoryName(storePath);
                _storeFileName = Path.GetFileName(storePath);
            }
        }

        public event EventHandler<FileAddedEventArgs<T>> FileAdded;
        public event EventHandler<FileRemovedEventArgs<T>> FileRemoved;
        public event EventHandler<StoreChangedEventArgs<T>> StoreChanged;
        public event EventHandler<StoreSavedEventArgs<T>> StoreSaved;
        public event EventHandler<StoreLoadedEventArgs<T>> StoreLoaded;

        public int Count => _storeFiles.Count;

        public bool IsReadOnly => false;

        public string Json { get; private set; } = String.Empty;

        public string StoreDirectory
        {
            get => _storeDirectory;

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(StoreDirectory));

                _storeDirectory = value;
            }
        }

        public string StoreFileName
        {
            get => _storeFileName;

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(StoreFileName));

                _storeFileName = value;
            }
        }

        public string StorePath => Path.Combine(StoreDirectory, StoreFileName);

        public void Add(T file)
        {
            _storeFiles.Add(file);

            FileAdded?.Invoke(this, new FileAddedEventArgs<T> { File = file });
            StoreChanged?.Invoke(this, new StoreChangedEventArgs<T>
            {
                Files = _storeFiles.AsReadOnly(),
                ChangesSummary = nameof(Add)
            });
        }

        public void Clear()
        {
            _storeFiles.Clear();

            StoreChanged?.Invoke(this, new StoreChangedEventArgs<T>
            {
                Files = new T[] { },
                ChangesSummary = nameof(Clear)
            });
        }

        public bool Contains(T file) => _storeFiles.Contains(file);

        public void CopyTo(T[] array, int arrayIndex) => _storeFiles.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _storeFiles.GetEnumerator();

        public bool Remove(T file)
        {
            bool success = _storeFiles.Remove(file);

            FileRemoved?.Invoke(this, new FileRemovedEventArgs<T> { File = file });
            StoreChanged?.Invoke(this, new StoreChangedEventArgs<T>
            {
                Files = _storeFiles.AsReadOnly(),
                ChangesSummary = nameof(Remove)
            });

            return success;
        }

        public void Save()
        {
            bool success = true;
            try
            {
                Json = JsonConvert.SerializeObject(_storeFiles);
                Directory.CreateDirectory(StoreDirectory);
                File.WriteAllText(StorePath, Json);
            }
            catch { success = false; }

            StoreSaved?.Invoke(this, new StoreSavedEventArgs<T>
            {
                Files = _storeFiles.AsReadOnly(),
                IsSuccessful = success
            });
        }

        public void ClearAndSave()
        {
            Clear();
            Save();
        }

        public void Load()
        {
            var files = new List<T>();

            bool success = true;
            try
            {
                Json = File.ReadAllText(StorePath);
                files = JsonConvert.DeserializeObject<List<T>>(Json);
            }
            catch { success = false; }

            _storeFiles = success ? files : _storeFiles;

            StoreLoaded?.Invoke(this, new StoreLoadedEventArgs<T>
            {
                Files = _storeFiles.AsReadOnly(),
                IsSuccessful = success
            });
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_storeFiles).GetEnumerator();
    }

    public class FileAddedEventArgs<T> : EventArgs
    {
        public T File { get; set; }
    }

    public class FileRemovedEventArgs<T> : EventArgs
    {
        public T File { get; set; }
    }

    public class StoreChangedEventArgs<T> : EventArgs
    {
        public IReadOnlyCollection<T> Files { get; set; }

        public string ChangesSummary { get; set; }
    }

    public class StoreSavedEventArgs<T> : EventArgs
    {
        public IReadOnlyCollection<T> Files { get; set; }

        public bool IsSuccessful { get; set; }
    }

    public class StoreLoadedEventArgs<T> : EventArgs
    {
        public IReadOnlyCollection<T> Files { get; set; }

        public bool IsSuccessful { get; set; }
    }
}

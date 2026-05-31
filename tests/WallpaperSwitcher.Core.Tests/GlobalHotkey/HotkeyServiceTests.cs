using WallpaperSwitcher.Core.Exceptions;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.Tests.GlobalHotkey;

public class HotkeyServiceTests
{
    [Test]
    public void RegisterHotkey_WithValidString_RegistersAndReturnsGeneratedId()
    {
        var registrar = new FakeHotkeyRegistrar();
        var service = CreateService(registrar);

        var id = service.RegisterHotkey("Ctrl+Alt+N", "Next");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(id, Is.EqualTo(1000));
            Assert.That(registrar.RegisteredIds, Does.Contain(1000));
            Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Next")?.Hotkey,
                Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.N)));
        }
    }

    [Test]
    public void RegisterHotkey_WithExplicitId_UsesProvidedId()
    {
        var service = CreateService();

        var id = service.RegisterHotkey("Ctrl+Alt+A", "Folder", 42);

        Assert.That(id, Is.EqualTo(42));
    }

    [Test]
    public void RegisterHotkey_WithDuplicateId_ThrowsAndKeepsExistingBinding()
    {
        var service = CreateService();
        service.RegisterHotkey("Ctrl+Alt+A", "Folder", 42);

        Assert.Throws<HotkeyBindingException>(() => service.RegisterHotkey("Ctrl+Alt+B", "Other", 42));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
            Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder")?.Id, Is.EqualTo(42));
        }
    }

    [Test]
    public void RegisterHotkey_WithDuplicateCombination_ThrowsWithExistingHotkey()
    {
        var service = CreateService();
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");

        var exception = Assert.Throws<HotkeyDuplicateBindingException>(() =>
            service.RegisterHotkey("Ctrl+Alt+A", "Other"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.ExistingHotkey.Name, Is.EqualTo("Folder"));
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
        }
    }

    [Test]
    public void UnregisterHotkey_WhenRegistrarSucceeds_RemovesBindingAndReturnsTrue()
    {
        var registrar = new FakeHotkeyRegistrar();
        var service = CreateService(registrar);
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");

        var result = service.UnregisterHotkey("Folder");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(registrar.RegisteredIds, Is.Empty);
            Assert.That(service.GetRegisteredHotkeys(), Is.Empty);
        }
    }

    [Test]
    public void UnregisterHotkey_WhenRegistrarFails_KeepsBindingAndReturnsFalse()
    {
        var registrar = new FakeHotkeyRegistrar();
        var service = CreateService(registrar);
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");
        registrar.UnregisterResults.Enqueue(false);

        var result = service.UnregisterHotkey("Folder");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(registrar.RegisteredIds, Does.Contain(1000));
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
        }
    }

    [Test]
    public void ChangeHotkeyBinding_WithBlankText_RemovesBinding()
    {
        var service = CreateService();
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");

        service.ChangeHotkeyBinding("Folder", "   ");

        Assert.That(service.GetRegisteredHotkeys(), Is.Empty);
    }

    [Test]
    public void ChangeHotkeyBinding_WithNewHotkey_PreservesExistingId()
    {
        var registrar = new FakeHotkeyRegistrar();
        var service = CreateService(registrar);
        service.RegisterHotkey("Ctrl+Alt+A", "Folder", 42);

        service.ChangeHotkeyBinding("Folder", "Ctrl+Alt+B");

        var hotkeyInfo = service.GetHotKeyInfoBy(h => h.Name, "Folder");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hotkeyInfo?.Id, Is.EqualTo(42));
            Assert.That(hotkeyInfo?.Hotkey, Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.B)));
            Assert.That(registrar.RegisterCalls.Select(call => call.Id), Does.Contain(42));
        }
    }

    [Test]
    public void ChangeHotkeyBinding_WithInvalidHotkey_LeavesOldBinding()
    {
        var service = CreateService();
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");

        Assert.Throws<HotkeyParsingException>(() => service.ChangeHotkeyBinding("Folder", "B"));

        Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder")?.Hotkey,
            Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A)));
    }

    [Test]
    public void ChangeHotkeyBinding_WithDuplicateHotkey_LeavesOldBinding()
    {
        var service = CreateService();
        service.RegisterHotkey("Ctrl+Alt+A", "Folder 1");
        service.RegisterHotkey("Ctrl+Alt+B", "Folder 2");

        Assert.Throws<HotkeyDuplicateBindingException>(() =>
            service.ChangeHotkeyBinding("Folder 2", "Ctrl+Alt+A"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder 1")?.Hotkey,
                Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A)));
            Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder 2")?.Hotkey,
                Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.B)));
        }
    }

    [Test]
    public void ChangeHotkeyBinding_WhenNewRegistrationFails_RestoresOldBinding()
    {
        var registrar = new FakeHotkeyRegistrar();
        var service = CreateService(registrar);
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");
        registrar.RegisterResults.Enqueue(false);

        Assert.Throws<HotkeyBindingException>(() => service.ChangeHotkeyBinding("Folder", "Ctrl+Alt+B"));

        Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder")?.Hotkey,
            Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A)));
    }

    [Test]
    public void LoadHotkeys_WithPersistedIds_AdvancesNextGeneratedId()
    {
        var storage = new FakeHotkeyStorage
        {
            Exists = true,
            Hotkeys =
            [
                new HotkeyInfo
                {
                    Id = 1000,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A),
                    Name = "A"
                },
                new HotkeyInfo
                {
                    Id = 1005,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.B),
                    Name = "B"
                }
            ]
        };
        var service = CreateService(storage: storage);

        service.LoadHotkeys();
        var newId = service.RegisterHotkey("Ctrl+Alt+C", "C");

        Assert.That(newId, Is.EqualTo(1006));
    }

    [Test]
    public async Task LoadHotkeysAsync_WithPersistedIds_AdvancesNextGeneratedId()
    {
        var storage = new FakeHotkeyStorage
        {
            Exists = true,
            Hotkeys =
            [
                new HotkeyInfo
                {
                    Id = 1000,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A),
                    Name = "A"
                },
                new HotkeyInfo
                {
                    Id = 1005,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.B),
                    Name = "B"
                }
            ]
        };
        var service = CreateService(storage: storage);

        await service.LoadHotkeysAsync();
        var newId = service.RegisterHotkey("Ctrl+Alt+C", "C");

        Assert.That(newId, Is.EqualTo(1006));
    }

    [Test]
    public void LoadHotkeys_WhenPersistedHotkeyRegistrationFails_SkipsFailureAndSavesSuccessfulHotkeys()
    {
        var registrar = new FakeHotkeyRegistrar();
        registrar.RegisterResults.Enqueue(true);
        registrar.RegisterResults.Enqueue(false);
        var storage = new FakeHotkeyStorage
        {
            Exists = true,
            Hotkeys =
            [
                new HotkeyInfo
                {
                    Id = 1000,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.A),
                    Name = "Folder"
                },
                new HotkeyInfo
                {
                    Id = 1001,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.W),
                    Name = Default.NextWallpaperHotkeyName
                }
            ]
        };
        var service = CreateService(registrar, storage);

        var result = service.LoadHotkeys();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasFailures, Is.True);
            Assert.That(result.Failures, Has.Exactly(1).Items);
            Assert.That(result.Failures[0].HotkeyInfo.Name, Is.EqualTo(Default.NextWallpaperHotkeyName));
            Assert.That(result.Failures[0].HotkeyInfo.Hotkey,
                Is.EqualTo(new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.W)));
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
            Assert.That(service.GetHotKeyInfoBy(h => h.Name, "Folder"), Is.Not.Null);
            Assert.That(storage.SaveCount, Is.EqualTo(1));
            Assert.That(storage.SavedHotkeys, Has.Exactly(1).Items);
            Assert.That(storage.SavedHotkeys[0].Name, Is.EqualTo("Folder"));
        }
    }

    [Test]
    public async Task LoadHotkeysAsync_WhenPersistedHotkeyRegistrationFails_SkipsFailureAndSavesSuccessfulHotkeys()
    {
        var registrar = new FakeHotkeyRegistrar();
        registrar.RegisterResults.Enqueue(false);
        var storage = new FakeHotkeyStorage
        {
            Exists = true,
            Hotkeys =
            [
                new HotkeyInfo
                {
                    Id = 1000,
                    Hotkey = new Hotkey(ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.W),
                    Name = Default.NextWallpaperHotkeyName
                }
            ]
        };
        var service = CreateService(registrar, storage);

        var result = await service.LoadHotkeysAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasFailures, Is.True);
            Assert.That(result.Failures, Has.Exactly(1).Items);
            Assert.That(service.GetRegisteredHotkeys(), Is.Empty);
            Assert.That(storage.SaveCount, Is.EqualTo(1));
            Assert.That(storage.SavedHotkeys, Is.Empty);
        }
    }

    [Test]
    public void LoadHotkeys_WhenStorageFileExistsButIsEmpty_DoesNotRegisterDefaultHotkey()
    {
        var storage = new FakeHotkeyStorage { Exists = true };
        var service = CreateService(storage: storage);

        var result = service.LoadHotkeys();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasFailures, Is.False);
            Assert.That(service.GetRegisteredHotkeys(), Is.Empty);
            Assert.That(storage.SaveCount, Is.Zero);
        }
    }

    [Test]
    public void LoadHotkeys_WhenStorageIsEmpty_RegistersAndSavesDefaultHotkey()
    {
        var storage = new FakeHotkeyStorage();
        var service = CreateService(storage: storage);

        var result = service.LoadHotkeys();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasFailures, Is.False);
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
            Assert.That(storage.SaveCount, Is.EqualTo(1));
            Assert.That(storage.SavedHotkeys, Has.Exactly(1).Items);
            Assert.That(storage.SavedHotkeys[0].Name, Is.EqualTo(Default.NextWallpaperHotkeyName));
        }
    }

    [Test]
    public async Task LoadHotkeysAsync_WhenStorageIsEmpty_RegistersAndSavesDefaultHotkey()
    {
        var storage = new FakeHotkeyStorage();
        var service = CreateService(storage: storage);

        var result = await service.LoadHotkeysAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasFailures, Is.False);
            Assert.That(service.GetRegisteredHotkeys(), Has.Exactly(1).Items);
            Assert.That(storage.SaveCount, Is.EqualTo(1));
            Assert.That(storage.SavedHotkeys, Has.Exactly(1).Items);
            Assert.That(storage.SavedHotkeys[0].Name, Is.EqualTo(Default.NextWallpaperHotkeyName));
        }
    }

    [Test]
    public void ProcessWindowMessage_ForRegisteredId_RaisesHotkeyPressed()
    {
        var service = CreateService();
        var id = service.RegisterHotkey("Ctrl+Alt+A", "Folder");
        HotkeyInfo? pressedHotkey = null;
        service.HotkeyPressed += (_, args) => pressedHotkey = args.HotkeyInfo;

        service.ProcessWindowMessage(id);

        Assert.That(pressedHotkey?.Name, Is.EqualTo("Folder"));
    }

    [Test]
    public async Task SaveHotkeysAsync_SavesCurrentSnapshot()
    {
        var storage = new FakeHotkeyStorage();
        var service = CreateService(storage: storage);
        service.RegisterHotkey("Ctrl+Alt+A", "Folder");

        await service.SaveHotkeysAsync();

        Assert.That(storage.SavedHotkeys, Has.Exactly(1).Items);
    }

    private static HotkeyService CreateService(
        FakeHotkeyRegistrar? registrar = null,
        FakeHotkeyStorage? storage = null)
    {
        return new HotkeyService(
            registrar ?? new FakeHotkeyRegistrar(),
            storage ?? new FakeHotkeyStorage()
        );
    }

    private sealed class FakeHotkeyRegistrar() : HotkeyRegistrar(new IntPtr(1))
    {
        public List<(int Id, ModifierKeys Modifiers, VirtualKeys Key)> RegisterCalls { get; } = [];

        public HashSet<int> RegisteredIds { get; } = [];

        public Queue<bool> RegisterResults { get; } = [];

        public Queue<bool> UnregisterResults { get; } = [];

        public override bool RegisterHotKey(int id, ModifierKeys modifiers, VirtualKeys key)
        {
            RegisterCalls.Add((id, modifiers, key));
            var result = RegisterResults.Count > 0 ? RegisterResults.Dequeue() : true;
            if (result)
            {
                RegisteredIds.Add(id);
            }

            return result;
        }

        public override bool UnregisterHotKey(int id)
        {
            var result = UnregisterResults.Count > 0 ? UnregisterResults.Dequeue() : true;
            if (result)
            {
                RegisteredIds.Remove(id);
            }

            return result;
        }
    }

    private sealed class FakeHotkeyStorage : IHotkeyStorage
    {
        public bool Exists { get; init; }

        public List<HotkeyInfo> Hotkeys { get; init; } = [];

        public List<HotkeyInfo> SavedHotkeys { get; private set; } = [];

        public int SaveCount { get; private set; }

        public Task<IEnumerable<HotkeyInfo>> LoadAsync()
        {
            return Task.FromResult<IEnumerable<HotkeyInfo>>(Hotkeys.ToArray());
        }

        public IEnumerable<HotkeyInfo> Load()
        {
            return Hotkeys.ToArray();
        }

        public Task SaveAsync(IEnumerable<HotkeyInfo> hotkeys)
        {
            SaveCount++;
            SavedHotkeys = hotkeys.ToList();
            return Task.CompletedTask;
        }

        public void Save(IEnumerable<HotkeyInfo> hotkeys)
        {
            SaveCount++;
            SavedHotkeys = hotkeys.ToList();
        }
    }
}

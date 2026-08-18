using System;
using System.IO;
using System.Text.Json;

namespace SincroDatosApp
{
    // Clase singleton para consultar par�metros desde appconfig.json
    public sealed class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _lazy = new(() => new ConfigManager());
        public static ConfigManager Instance => _lazy.Value;

        private readonly string _path;
        private readonly object _lock = new();
        private AppConfig _config = new AppConfig();

        private ConfigManager()
        {
            _path = Path.Combine(AppContext.BaseDirectory, "appconfig.json");
            Load();
        }

        private void Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_path))
                    {
                        _config = new AppConfig();
                        Save();
                        return;
                    }

                    var json = File.ReadAllText(_path);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                    if (cfg != null)
                    {
                        _config = cfg;
                    }
                }
                catch
                {
                    // En caso de error, mantener valores por defecto en _config
                    _config = new AppConfig();
                }
            }
        }

        public AppConfig GetConfig()
        {
            lock (_lock)
            {
                // devolver copia para evitar modificaciones externas accidentales
                return new AppConfig
                {
                    ApiUrl = _config.ApiUrl,
                    TxInterval = _config.TxInterval,
                    EnableLogging = _config.EnableLogging,
                    PathArchivoTx = _config.PathArchivoTx,
                    UrlImagenBase = _config.UrlImagenBase,
                    ApiUsuario = _config.ApiUsuario,
                    ApiClave = _config.ApiClave
                };
            }
        }

        public string ApiUrl => GetConfig().ApiUrl;
        public int TxInterval => GetConfig().TxInterval;
        public bool EnableLogging => GetConfig().EnableLogging;
        public string PathArchivoTx => GetConfig().PathArchivoTx;
        public string UrlImagenBase => GetConfig().UrlImagenBase;
        public string ApiUsuario => GetConfig().ApiUsuario;
        public string ApiClave => GetConfig().ApiClave;
        public void Reload()
        {
            Load();
        }

        public void Save()
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
        }

        public void Update(Action<AppConfig> updater)
        {
            if (updater == null) return;
            lock (_lock)
            {
                updater(_config);
                Save();
            }
        }
    }
}

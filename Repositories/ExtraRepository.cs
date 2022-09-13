using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Repositories
{
    public sealed class ExtraRepository : IExtraRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private List<SmallExtra> SmallExtras { get; set; }
        public ExtraRepository(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;
            SmallExtras = new();
        }

        public async Task AcceptProposal(string text)
        {
            await DataQueries.Save("UPDATE small_extras SET status='NORMAL' WHERE text=@text", new { text });
        }
        public async Task RejectProposal(string text)
        {
            await DataQueries.Delete("DELETE FROM small_extras WHERE text=@text AND status='PROPOSAL'", new { text });
        }
        public async Task AddSmallExtraProposal(SmallExtra extra)
        {
            if (string.IsNullOrWhiteSpace(extra.Text)) throw new AppException("Der Text darf nicht leer sein");
            if (extra.Author is null) extra.Author = "";

            try
            {
                AttackDetector.Detect(extra.Text, extra.Author);
            }
            catch (AppException ex)
            {
                Logger.Warn(LogArea.Attack, ex, "Somebody tried to attack on small extra proposal");
                throw new AppException(ex.Message);
            }

            if (extra.Text.Length > 1024) throw new AppException("Der Text darf die Länge von 1024 Zeichen nicht überschreiten.");
            var matches = Regex.Matches(extra.Text, @"[^\wäöüÄÖÜß\s\-()[\]&%$§€!?.,;:+*/#""\'^°]");
            if (matches.Count > 0) throw new AppException($"Der Text beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");

            if (extra.Author.Length > 64) throw new AppException("Der Autor darf die Länge von 64 Zeichen nicht überschreiten.");
            matches = Regex.Matches(extra.Author, @"[^\wäöüÄÖÜß\s\-()&]");
            if (matches.Count > 0) throw new AppException($"Der Autor beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");
            
            await DataQueries.Save("INSERT INTO small_extras(text, author, status) VALUES (@text, @author, 'PROPOSAL')", new { text = extra.Text, author = extra.Author });
        }
        public async Task<List<SmallExtra>> GetProposals()
        {
            return await DataQueries.Load<SmallExtra, dynamic>("SELECT text, author FROM small_extras WHERE status='PROPOSAL'", new { });
        }

        public async Task<SmallExtra> GetRandSmallExtra()
        {
            if(SmallExtras.Count == 0)
            {
                await Preload();
            }

            if(SmallExtras.Count == 0)
            {
                Logger.Warn(LogArea.SmallExtraRepo, "Es wurde das Platzhalte Kleine Extra genutzt.", "");
                return new SmallExtra
                {
                    Text = "Einen wunderschönen Guten Tag! Eigentlich solltest du diesen Text nicht sehen, da er nur ein Platzhalter für das eigentliche kleine aber feine Extra ist...doof.",
                    Author = "Pascal"
                };
            }
            return SmallExtras[new Random().Next(SmallExtras.Count)];
        }
        private async Task Preload()
        {
            int MAX_COUNT = 100;
            try
            {
                SmallExtras = await DataQueries.Load<SmallExtra, dynamic>("SELECT text, author FROM small_extras WHERE status='NORMAL' ORDER BY RAND() LIMIT @limit", new { limit = MAX_COUNT });
            }catch(Exception ex)
            {
                SmallExtras = new();
                Logger.Error(LogArea.SmallExtraRepo, ex, "Tried to preload small extras.");
            }
        }

        
    }
}

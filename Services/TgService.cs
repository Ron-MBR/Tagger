using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tagger.Data;
using Tagger.Models;
using User = Tagger.Models.User;
using Tagger.Services;

namespace Tagger.Services
{
    public class TgService
    {
        private readonly TelegramBotClient _botClient;
        private readonly CancellationTokenSource _cts;
        private readonly UserDbContext _dbContext;
        private readonly StaffDbContext _staffDbContext;
        private readonly TagDbContext _tagDbContext;
        private readonly string genToken = "assguardmaster2007"; 
        private readonly string _genqrpath = @"C:\PROJECTS\dotnet\Tagger\Media\QRTALT.jpg";
        private readonly string _scanqrpath = @"C:\PROJECTS\dotnet\Tagger\Media\QRSCAN.jpg";
        private readonly string _genexpath = @"C:\PROJECTS\dotnet\Tagger\Media";
        private long _masterId;
        private readonly ExcelService _excelService;
        QRService qrService = new QRService();
        
        private readonly DynamicRequestsStore<long> _requests = new DynamicRequestsStore<long>();
        private readonly DynamicTokensStore _tokens = new DynamicTokensStore();
        private readonly DynamicCounterStore _counters = new DynamicCounterStore();
        private string GenerateToken(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string token = string.Empty;
            token = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
            
            return token;
        }

        public TgService(string token, UserDbContext usdbContext, StaffDbContext sfdbContext, TagDbContext tagdbContext)
        {
            _botClient = new TelegramBotClient(token);
            _staffDbContext = sfdbContext;
            _dbContext = usdbContext;
            _tagDbContext = tagdbContext;
            _cts = new CancellationTokenSource();
            _excelService = new ExcelService(_dbContext,_staffDbContext,_tagDbContext);
        }

        public async Task StartReceiving()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            _tagDbContext.Database.EnsureCreated();
            _tagDbContext.Database.Migrate();
            _staffDbContext.Database.EnsureCreated();
            _staffDbContext.Database.Migrate();
            _dbContext.Database.EnsureCreated();
            _dbContext.Database.Migrate();
            _botClient.StartReceiving(UpdateHandler,ErrorHandler);
        }

        public async Task StopReceiving()
        {
            _cts.Cancel();
            
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        MessageHandler(client, update.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        
        }
        
        public async Task MessageHandler(ITelegramBotClient botClient, Message message)
        {
            if (message?.Photo != null)
            {
                await HandlePhotoMessage(message);
            }
            
            if (message.Text is not { } messageText)
                return;

            switch (messageText)
            {
                case "/start":
                    {
                        var _users = await _dbContext.Users.Where(kk => kk.ChatId == message.Chat.Id).ToListAsync();
                        
                        if (_users.Any())
                        {
                            Console.WriteLine("Succes data invoke");
                        }
                        else
                        {
                            User us = new User
                            {
                                Id = Guid.NewGuid(),
                                ChatId = message.Chat.Id
                            };
                            try
                            {
                                await _dbContext.Users.AddAsync(us, _cts.Token);
                                await _dbContext.SaveChangesAsync(_cts.Token);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        _botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Введите свою фамилию"
                        );
                        _requests.Add(message.Chat.Id, 101);
                        break;
                    }
                case "/gigastart":
                {
                    var _staffs = await _staffDbContext.Staffs.Where(kk => kk.ChatId == message.Chat.Id).ToListAsync();
                    if (_staffs.Any())
                    {
                        Console.WriteLine("Succes data invoke");
                    }
                    else
                    {
                        Staff sf = new Staff
                        {
                            Id = Guid.NewGuid(),
                            ChatId = message.Chat.Id
                        };
                        try
                        {
                            await _staffDbContext.Staffs.AddAsync(sf, _cts.Token);
                            await _staffDbContext.SaveChangesAsync(_cts.Token);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    _botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Введите ген токен"
                    );
                    _requests.Add(message.Chat.Id, 110);
                    break;
                }
                case "/getallusers":
                {
                    if (await MasterValidator(message))
                    {
                        var usrs = await _dbContext.Users.ToListAsync();
                        foreach (User usr in usrs)
                        {
                            Console.WriteLine($"User: {usr.Surname} {usr.Name} {usr.Legacyname}, his Id: {usr.ChatId}");
                        }
                    }

                    break;
                }
                case "/getallstaffs":
                {
                    if (await MasterValidator(message))
                    {
                        var staffs = await _staffDbContext.Staffs.ToListAsync();
                        foreach (Staff stf in staffs)
                        {
                            Console.WriteLine($"User: {stf.LastName} {stf.FirstName} {stf.LegacyName}, his Id: {stf.ChatId}");
                        }
                    }

                    break;
                }
                case "/getalltags":
                {
                    if (await MasterValidator(message))
                    {
                        var tags = await _tagDbContext.Tags.ToListAsync();
                        foreach (Tag tag in tags)
                        {
                            Console.WriteLine($"User id: {tag.UserChatId} teacher id: {tag.StaffChatId} date: {tag.TagTime}");
                        }
                    }

                    _excelService.GenTagExcel(_genexpath);
                    await using var fileStream = File.OpenRead(Path.Combine(_genexpath, "tags.xlsx"));
                    await botClient.SendDocument(
                        chatId: message.Chat.Id,
                        document: InputFile.FromStream(fileStream),
                        caption: "Список отметок учеников",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );
                    break;
                }
                case "/tagging":
                {
                    if (await MasterValidator(message))
                    {
                        _counters.Add(message.Chat.Id);
                        await SendGenQr(message.Chat.Id,_counters.Get(message.Chat.Id));
                    }
                    break;
                }
                default:
                    {
                        int _code = _requests.Get(message.Chat.Id);
                        _requests.Remove(message.Chat.Id);
                        switch (_code)
                        {
                            case 101:
                            {
                                var usrs = await _dbContext.Users.FirstOrDefaultAsync(
                                    kk => kk.ChatId == message.Chat.Id, _cts.Token);
                                usrs.Surname = message.Text;
                                _dbContext.SaveChangesAsync(_cts.Token);
                                
                                _botClient.SendMessage(
                                    chatId: message.Chat.Id,
                                    text: "Введите имя");
                                _requests.Add(message.Chat.Id, 102);
                                break;
                            }
                            case 102:
                                {
                                    var usrs = await _dbContext.Users.FirstOrDefaultAsync(
                                        kk => kk.ChatId == message.Chat.Id, _cts.Token);
                                    usrs.Name = message.Text;
                                    _dbContext.SaveChangesAsync(_cts.Token);
                                    
                                    _botClient.SendMessage(
                                        chatId: message.Chat.Id,
                                        text: "Введите отчество"
                                    );
                                    _requests.Add(message.Chat.Id, 103);
                                    break;
                                }
                                case 103:
                                {
                                    var usrs = await _dbContext.Users.FirstOrDefaultAsync(
                                        kk => kk.ChatId == message.Chat.Id);
                                    usrs.Legacyname = message.Text;
                                    _dbContext.SaveChangesAsync(_cts.Token);
                                    
                                    _botClient.SendMessage(
                                        chatId: message.Chat.Id,
                                        text: "Спасибки"
                                    );
                                    break;
                                }
                            case 110:
                            {
                                if (message.Text == genToken)
                                {
                                    _requests.Add(message.Chat.Id, 111);
                                    _botClient.SendMessage(
                                        chatId: message.Chat.Id,
                                        text: "Введите свою фамилию"
                                    );
                                }
                                break;
                            }
                            case 111:
                            {
                                var usrs = await _staffDbContext.Staffs.FirstOrDefaultAsync(
                                    kk => kk.ChatId == message.Chat.Id);
                                usrs.LastName = message.Text;
                                _staffDbContext.SaveChangesAsync(_cts.Token);
                                
                                _botClient.SendMessage(
                                    chatId: message.Chat.Id,
                                    text: "Введите имя");
                                _requests.Add(message.Chat.Id, 112);
                                break;
                            }
                            case 112:
                            {
                                var usrs = await _staffDbContext.Staffs.FirstOrDefaultAsync(
                                    kk => kk.ChatId == message.Chat.Id);
                                usrs.FirstName = message.Text;
                                _staffDbContext.SaveChangesAsync(_cts.Token);
                                
                                _botClient.SendMessage(
                                    chatId: message.Chat.Id,
                                    text: "Введите отчество"
                                );
                                _requests.Add(message.Chat.Id, 113);
                                break;
                            }
                            case 113:
                            {
                                var usrs = await _staffDbContext.Staffs.FirstOrDefaultAsync(
                                    kk => kk.ChatId == message.Chat.Id);
                                usrs.LegacyName = message.Text;
                                _staffDbContext.SaveChangesAsync(_cts.Token);
                                
                                _botClient.SendMessage(
                                    chatId: message.Chat.Id,
                                    text: "Спасибки"
                                );
                                break;
                            }
                            default:
                                {
                                    _botClient.SendMessage(
                                        chatId: message.Chat.Id,
                                        text: "Не удалось распознать команду/текст"
                                    );
                                    break;
                                }
                        }
                        
                        break;
                    }
            }
        }
        public async Task SendTextMessageAsync(long chatId, string text)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: text
                );
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private async Task<bool> MasterValidator(Message msg)
        {
            var msUser = await _staffDbContext.Staffs.FirstOrDefaultAsync(k => k.ChatId == msg.Chat.Id);
            if(msUser != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task HandlePhotoMessage(Message message)
        {
            var photo = message.Photo;
            var fileId = photo[^1].FileId;
            await using var fileStream = new FileStream(_scanqrpath, FileMode.Create);
            var file = await _botClient.GetInfoAndDownloadFile(fileId, fileStream);
            fileStream.Close();
            var _tkn = qrService.ReadQrAlt(_scanqrpath);
            
            if (_tkn != null)
            {
                var teacherId = _tokens.Get(_tkn);
                if (teacherId != 0)
                {
                    if (teacherId == message.Chat.Id)
                    {
                        _tokens.Remove(_tkn);
                        _counters.Remove(message.Chat.Id);
                        _botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Вы закончили перекличку, токены удалены <3"
                        );
                    }
                    else
                    {
                        Tag _tag = new Tag
                        {
                            Id = Guid.NewGuid(),
                            UserChatId = message.Chat.Id,
                            StaffChatId = teacherId,
                            TagTime = DateTime.Now
                        };
                        await _tagDbContext.Tags.AddAsync(_tag);
                        await _tagDbContext.SaveChangesAsync();

                        _tokens.Remove(_tkn);

                        _botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Вы отмечены, хорошего дня)"
                        );
                        _counters.Push(teacherId);
                        await SendGenQr(teacherId,_counters.Get(teacherId));
                    }

                    return;
                }
                else
                {
                    _botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "QR недействителен"
                    );
                }
            }
            else
            {
                _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Не удалось распознать QR"
                );
            }
        }

        /*private async Task SendGenQrAlt(long teacherId)
        {
            var _nexttkn = GenerateToken(12);
            qrService.SaveQr(_nexttkn, 40, _genqrpath);
            _tokens.Add(_nexttkn, teacherId);
                            
            await using var ffileStream = new FileStream(_genqrpath, FileMode.Open, FileAccess.Read);
            var inputFfile = new InputFileStream(ffileStream, Path.GetFileName(_genqrpath));
            await _botClient.SendPhoto(teacherId, inputFfile);
        }*/
        
        private async Task SendGenQr(long teacherId, int? counter)
        {
            var _nexttkn = GenerateToken(12);
            _tokens.Add(_nexttkn, teacherId);

            MemoryStream streamchek = new MemoryStream(qrService.GenerateQr(_nexttkn,40));
            var inputFfile = new InputFileStream(streamchek, Path.GetFileName(_genqrpath));
            await _botClient.SendPhoto(teacherId, inputFfile,caption: counter.ToString());
                
        }
    }
}
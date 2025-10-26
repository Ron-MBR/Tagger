using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Tagger.Data;
using Tagger.Models;
namespace Tagger.Services
{
    public class ExcelService
    {
        private readonly UserDbContext _userDbContext;
        private readonly StaffDbContext _staffDbContext;
        private readonly TagDbContext _tagDbContext;
        
        public ExcelService(UserDbContext  userDbContext, StaffDbContext staffDbContext, TagDbContext tagDbContext)
        {
                _userDbContext = userDbContext;
                _staffDbContext = staffDbContext;
                _tagDbContext = tagDbContext;
        }

        public async Task GenTagExcel(string _path)
        {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Sheet1");

                int counter = 1;
                var _tags = await _tagDbContext.Tags.ToListAsync();
                IEnumerable<Tag> tags = _tags;
                worksheet.Cell(1, 1).Value = "Дата";
                worksheet.Cell(1, 2).Value = "ФИО ученика";
                worksheet.Cell(1, 3).Value = "ФИО учителя";
                foreach (var tag in tags)
                {
                    counter++;
                    var userok = await _userDbContext.Users.FirstOrDefaultAsync(kk => kk.ChatId == tag.UserChatId);
                    var stafferok =
                        await _staffDbContext.Staffs.FirstOrDefaultAsync(kk => kk.ChatId == tag.StaffChatId);
                    worksheet.Cell(counter, 1).Value = $"{tag.TagTime}";
                    worksheet.Cell(counter, 2).Value = $"{userok.Surname} {userok.Name} {userok.Surname}";
                    worksheet.Cell(counter, 3).Value =
                        $"{stafferok.LastName} {stafferok.FirstName} {stafferok.LegacyName}";
                }
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(Path.Combine(_path, "tags.xlsx"));
            
        }
    }
}
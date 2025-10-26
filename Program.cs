
using System.Data.Common;
using Tagger.Data;
using Tagger.Models;
using Tagger.Services;
// See https://aka.ms/new-console-template for more information

internal class Programm() {
    private static void Main()
    {
        var usdbContext = new UserDbContext();
        var sfdbContext = new StaffDbContext();
        var tagdbContext = new TagDbContext();
        var tgBotik = new TgService("8370931878:AAF5cD-q1Sog_AFj0Sv9C3G5T3vPcQHGOn0", usdbContext, sfdbContext, tagdbContext);
        tgBotik.StartReceiving();
        Console.ReadLine();
    }
}
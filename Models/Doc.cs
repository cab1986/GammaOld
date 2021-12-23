﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma
{
    public class Doc
    {
        /*public Doc()
        { }

        public Doc(Guid docID, string number, DateTime date, byte shiftID, string place, string user, string person, bool isConfirmed, int? docTypeID)
        {
            DocID = docID;
            Number = number;
            Date = date;
            ShiftID = shiftID;
            Place = place;
            User = user;
            Person = person;
            IsConfirmed = isConfirmed;
            DocTypeID = docTypeID;
        }*/
        public Guid DocID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public byte ShiftID { get; set; }
        public string Place { get; set; }
        public string User { get; set; }
        public string Person { get; set; }
        public bool IsConfirmed { get; set; }
        public int? DocTypeID { get; set; }
        public string Comment { get; set; }
    }
}

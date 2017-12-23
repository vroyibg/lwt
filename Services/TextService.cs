using System.Collections.Generic;
using System.Linq;
using LWT.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LWT.Services
{
    public class TextService : ITextService
    {
        private LWTContext _context;
        private readonly ITermService _termService;
        private readonly ITextTermService _textTermService;
        public TextService(LWTContext context, ITermService termService, ITextTermService textTermService)
        {
            _context = context;
            _textTermService = textTermService;
            _termService = termService;
        }
        // Add a text to database
        public void Add(Text text)
        {
            _context.Text.Add(text);
            _context.SaveChanges();
        }

        // Get all texts from database
        public List<Text> GetAll()
        {
            return _context.Text.Include(text => text.Language).ToList();
        }

        // get the text coresponding to given id
        public Text GetByID(int id)
        {
            return _context.Text.Include(text => text.Language)
            .Include(text => text.Terms)
            .FirstOrDefault(text => text.ID == id);
        }

        // check if a text with given id exist
        public bool IsExist(int id)
        {
            return _context.Text.Any(text => text.ID == id);
        }

        // remove a text from database
        public void Delete(Text text)
        {
            if (IsExist(text.ID))
            {
                _context.Text.Remove(text);
                _context.SaveChanges();
            }
        }

        // update a text
        public void Update(Text text)
        {
            if (IsExist(text.ID))
            {
                _context.Text.Update(text);
                _context.SaveChanges();
            }
        }

        // Parse the text
        public void Parse(int id)
        {
            // get the text
            Text text = GetByID(id);
            // get the word split pattern of the language
            // of the text
            string wordSplitPattern = text.Language.WordSplitPattern;
            // split the text
            Regex wordSplitter = new Regex($"(\\{wordSplitPattern})");
            string[] words = wordSplitter.Split(text.Content);

            // Clear the present terms for reparse
            _textTermService.DeleteRange(text.Terms);


            // Add terms to the text
            for (int index = 0; index < words.Length; index++)
            {
                string word = words[index];
                Term term;

                // check if the term is exist in database
                term = _termService.GetByContentAndLanguage(word, text.Language);
                if (term == null)
                {
                    // create new term
                    if (wordSplitter.Match(word).Success)
                    {
                        // This term is not exist in the language set it to don't care
                        term = new Term() { Content = word, Level = -2, Language = text.Language };
                    }
                    else
                    {
                        // Create an unknow term
                        term = new Term() { Content = word, Level = 0, Language = text.Language };
                    }
                    _termService.Add(term);
                }
                // create the link
                TextTerm textTerm = new TextTerm { TextID = text.ID, TermID = term.ID, Term = term, Text = text, TermIndex = index };
                _textTermService.Add(textTerm);
                // add the text the containing list of term
                term.ContainingTexts.Add(textTerm);
                //update the term
                _termService.Update(term);
            }
            Update(text);
        }
    }
}
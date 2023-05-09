using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class SelectLanguage : MonoBehaviour
    {
        public Text create;
        public Text delete;
        public Text edit;
        public Text refresh;

        public Text sortBy;
        public Text sortByTitle;
        public Text sortByArtist;

        public Text createKeyword;
        public Text createTitle;
        public Text createArtist;
        public Text createBPM;
        public Text createHint;
        public Text createCreate;
        public Text createClean;
        public Text createAutoFill;
        public Text createBatch;

        public Text deleteKeyword;
        public Text deleteDelete;
        public Text deleteClean;
        public Text deleteDeleteFile;

        public Text editKeyword;
        public Text editTitle;
        public Text editArtist;
        public Text editBPM;
        public Text editHint;
        public Text editEdit;
        public Text editClean;

        public Text selectDiff;
        public Text selectAuto;
        public Text selectMirror;
        public Text selectHard;
        public Text selectDeleteHi;
        public Text selectStart;
        public Text selectSkillCheck;
        public Text selectHPBar;

        // Start is called before the first frame update
        void Awake()
        {
            create.text = LanguageManager.Instance.GetText("select.create");
            delete.text = LanguageManager.Instance.GetText("select.delete");
            edit.text = LanguageManager.Instance.GetText("select.edit");
            refresh.text = LanguageManager.Instance.GetText("select.refresh");
            sortBy.text = LanguageManager.Instance.GetText("select.sortby");
            sortByTitle.text = LanguageManager.Instance.GetText("select.sortby.title");
            sortByArtist.text = LanguageManager.Instance.GetText("select.sortby.artist");
            createKeyword.text = LanguageManager.Instance.GetText("select.create.keyword");
            createTitle.text = LanguageManager.Instance.GetText("select.create.title");
            createArtist.text = LanguageManager.Instance.GetText("select.create.artist");
            createBPM.text = LanguageManager.Instance.GetText("select.create.bpm");
            createHint.text = LanguageManager.Instance.GetText("select.create.hint");
            createCreate.text = LanguageManager.Instance.GetText("select.create.create");
            createClean.text = LanguageManager.Instance.GetText("select.create.clean");
            createAutoFill.text = LanguageManager.Instance.GetText("select.create.autofill");
            createBatch.text = LanguageManager.Instance.GetText("select.create.batch");
            deleteKeyword.text = LanguageManager.Instance.GetText("select.delete.keyword");
            deleteDelete.text = LanguageManager.Instance.GetText("select.delete.delete");
            deleteClean.text = LanguageManager.Instance.GetText("select.delete.clean");
            deleteDeleteFile.text = LanguageManager.Instance.GetText("select.delete.deletefile");
            editKeyword.text = LanguageManager.Instance.GetText("select.edit.keyword");
            editTitle.text = LanguageManager.Instance.GetText("select.edit.title");
            editArtist.text = LanguageManager.Instance.GetText("select.edit.artist");
            editBPM.text = LanguageManager.Instance.GetText("select.edit.bpm");
            editHint.text = LanguageManager.Instance.GetText("select.edit.hint");
            editEdit.text = LanguageManager.Instance.GetText("select.edit.edit");
            editClean.text = LanguageManager.Instance.GetText("select.edit.clean");
            selectDiff.text = "> " + LanguageManager.Instance.GetText("select.diffselection");
            selectAuto.text = LanguageManager.Instance.GetText("select.auto");
            selectMirror.text = LanguageManager.Instance.GetText("select.mirror");
            selectHard.text = LanguageManager.Instance.GetText("select.hard");
            selectDeleteHi.text = LanguageManager.Instance.GetText("select.deletehi");
            selectStart.text = LanguageManager.Instance.GetText("select.start");
            selectSkillCheck.text = LanguageManager.Instance.GetText("select.skillcheck");
            selectHPBar.text = LanguageManager.Instance.GetText("select.hpbar");
        }
    }
}
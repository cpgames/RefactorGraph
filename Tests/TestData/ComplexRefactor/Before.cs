using cpGames.core.RapidIoC;
using ECS;
using PAShared.data;

namespace PAServer.roles
{
    public class ReturnFromEncounterRequestModel : RequestWithEmptyResponseModel
    {
        #region Fields
        public CpPeer peer;
        public Id encounterId;
        #endregion
    }

    public class ReturnFromEncounterRequest : RequestWithEmptyResponse<ReturnFromEncounterRequestModel>
    {
        #region Constructors
        public ReturnFromEncounterRequest(ReturnFromEncounterRequestModel model) : base(model) { }
        #endregion

        #region Methods
        protected override void Execute()
        {
            if (!Rapid.GetBindingValue<Entity>(GameIds.WorldKey, out var worldEntity, out var errorMessage) ||
                !worldEntity.GetChildById(GameIds.ENCOUNTER, out var encounterManagerEntity, out errorMessage) ||
                !encounterManagerEntity.GetChildById(_model.encounterId, out var encounterEntity, out errorMessage) ||
                !encounterEntity.GetComponent<IEncounter>(out var encounter, out errorMessage) ||
                !encounter.Body.GetTargetComponent<IBody>(out var body, out errorMessage) ||
                !_model.peer.AddRole(new RoleModel(RoleType.Sector, body.Sector.Get()), out errorMessage))
            {
                ExecuteFail(errorMessage);
                return;
            }

            ExecuteSuccess();
        }
        #endregion
    }
}
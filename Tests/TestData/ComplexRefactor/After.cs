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
            var addRoleResult = Rapid.GetBindingValue<Entity>(GameIds.WorldKey, out var worldEntity) &&
                worldEntity.GetChildById(GameIds.ENCOUNTER, out var encounterManagerEntity) &&
                encounterManagerEntity.GetChildById(_model.encounterId, out var encounterEntity) &&
                encounterEntity.GetComponent<IEncounter>(out var encounter) &&
                encounter.Body.GetTargetComponent<IBody>(out var body) &&
                _model.peer.AddRole(new RoleModel(RoleType.Sector, body.Sector.Get()));
            if (!addRoleResult)
            {
                ExecuteFail(addRoleResult.ErrorMessage);
                return;
            }

            ExecuteSuccess();
        }
        #endregion
    }
}
using NUnit.Framework;
using System;
using UHTN;
using UHTN.Builder;

namespace Tests
{
    public class WsFieldTypeTest
    {
        private enum WorldStates
        {
            [WsFieldHint(typeof(WsFieldInt))]
            Int,

            [WsFieldHint(typeof(WsFieldBool))]
            Bool,

            [WsFieldHint(typeof(WsFieldEnum<Colors>))]
            Enum,
        }

        public enum Colors
        {
            Red,
            Blue,
            Yellow
        }

        private EnumWorldState<WorldStates> _worldState;

        [SetUp]
        public void SetUp()
        {
            var description = EnumWorldState<WorldStates>.CreateDescription();
            _worldState = new EnumWorldState<WorldStates>(description.CreateWorldState());
        }

        [TestCase(1)]
        [TestCase(-3)]
        [TestCase(int.MaxValue)]
        public void IntValidateSuccessTest(int value)
        {
            _worldState.SetInt(WorldStates.Int, value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BoolValidateSuccessTest(bool value)
        {
            _worldState.SetBool(WorldStates.Bool, value);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void BoolValidateFailTest(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _worldState.SetInt(WorldStates.Bool, value));
        }

        [TestCase(Colors.Red)]
        [TestCase(Colors.Blue)]
        [TestCase(Colors.Yellow)]
        public void EnumValidateSuccessTest(Colors value)
        {
            _worldState.SetEnum(WorldStates.Enum, value);
        }

        [TestCase(4)]
        [TestCase(-1)]
        public void EnumValidateFailTest(Colors value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _worldState.SetEnum(WorldStates.Enum, value));
        }
    }
}

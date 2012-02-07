require 'minitest/autorun'
require File.join(File.dirname(File.expand_path(__FILE__)), '..', 'example.rb')
#require '../example.rb'

class TestSpotterAPI < MiniTest::Unit::TestCase
  def setup
    @spotter = SpotterAPI.new('localhost', 5000)
  end

  def test_get
    res = @spotter.get_resource('gps.json')
    assert_instance_of(Hash, res)
    assert(!res['error'])
  end

  def test_settings
    res = @spotter.set_settings('sensor.json', {'detection_min_velocity' => 3 })
    assert(!res['error'])
    assert(res['success'])
  end
end

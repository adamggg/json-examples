require 'json'
require 'net/http'

class SpotterAPI
  attr_accessor :spotter_url, :spotter_port

  # Create a new instance of the API pointed at a specific unit.
  def initialize(spotter_url, spotter_port=80)
    @spotter_url = spotter_url
    @spotter_port = spotter_port
    @http = Net::HTTP.new(@spotter_url, @spotter_port)
  end

  # Get a resource specified by resource_name string.
  # For example:
  #   foo = SpotterAPI.new('some.spotter.url.com')
  #   foo.get_resource 'sensor.json'
  # Returns a hash.
  def get_resource(resource_name)
    resource_name = format_resource_name(resource_name)
    begin
      JSON.parse(Net::HTTP.get(@spotter_url, resource_name, @spotter_port))
    rescue EOFError => e
      $stderr.print("Could not get resource. Unit may be down, or the URL could be malformed.")
      raise
    end
  end

  def get_settings(resource_name)
    resource_name = format_resource_name(resource_name)
    begin
      JSON.parse(Net::HTTP.get(@spotter_url, "#{resource_name}/settings", @spotter_port))
    rescue EOFError => e
      $stderr.print("Could not get resource. Unit may be down, or the URL could be malformed.")
      raise
    end
  end

  # Set the settings of a specific resource.
  def set_settings(resource_name, settings_hash)
    resource_name = format_resource_name(resource_name) + '/settings'

    # Turn hashes into escaped JSON strings
    settings_hash = JSON.generate(settings_hash) unless settings_hash.kind_of? String

    req = Net::HTTP::Post.new(resource_name)
    req['Content-Type'] = 'application/json'
    req.body = settings_hash
    response = Net::HTTP.new(@spotter_url, @spotter_port).start do |http|
      http.request(req)
    end
    JSON.parse(response.body)
  end

  private
  def format_resource_name(resource_name)
    resource_name = '/' + resource_name if resource_name[0] != '/'
  end
end

# Example usage
spotter = SpotterAPI.new('some.spotter.url.com')

# get a resource, and loop over it. Comes back as a Hash
tracks = spotter.get_resource('tracks.json')

tracks['result'].each do |result|
  result['observation'].each do |key, value|
    puts "result #{key} = #{value}"
  end
end

result = spotter.set_settings('gps.json', {'useGpsTime' => true})
# should be true if it worked
puts result['success']

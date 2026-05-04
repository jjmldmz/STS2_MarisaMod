shader_type canvas_item;
render_mode blend_add;

//颜色
uniform float Saturation = 0.2f;
uniform vec4 spark_color : source_color = vec4(0.5, 0.3, 0.1, 1.0);

uniform sampler2D noise_tex;
uniform float projectile_speed : hint_range(0.0, 1.0) = 1.0;
uniform	float turn_amount : hint_range(0.0, 1.0) = 0.0;
uniform float visible = 1.0;
float lineNoise(float phase)//phase的输入应当是正负PI
{
	//
	float x = abs(phase/(PI))  ;
	x = mod(x - 0.1 *TIME,1.0);
	float period = 30.0;
	float y = mod(TIME,period)/period;
	float noise =texture(noise_tex, vec2(x,y)).r;
	float noise2 = texture(noise_tex, vec2(mod(x + 0.5,1.0),y)).r;
	return mix(noise,noise2,0.5);
	//return noise2;
}
//c的x,y,z取值范围均为0~1
vec3 hsv2rgb(vec3 c){
    vec3 rgb = clamp(abs(mod(c.x*6.0 + vec3(0,4,2),6.0)-3.0)-1.0,0.0,1.0);
    return c.z * mix(vec3(1.0), rgb, c.y);
}
//归一化亮度
vec3 norm_col(vec3 c){
	float lum = dot(c, vec3(0.2126, 0.7152, 0.0722));
	c /= (lum + 1e-4);
	return c;
}

float decay(float d, float f1, float f2,float f3)
{
	//f1,f2共同决定边界半径，f3决定辉光散布
	float result = pow(1.0 / (d * f1 + f2), f3);
	result -= pow(1.0 / (1.0 * f1 + f2), f3);//确保边界处辉光归零
	return result;
}
void fragment()
{
    vec2 uv = (UV - 0.5) * 2.0;
    float r = length(uv);//0~1
    float angle = atan(uv.y, uv.x);
	float noise_scale = projectile_speed * 0.03*abs(angle)/PI + 0.08;
	float d = r + noise_scale * lineNoise( angle);
	float decay_f1 = 9.0;
	float decay_f2 =0.02;
	float stretch_coeff = projectile_speed * mix(1.0,0.5,turn_amount);
	decay_f1 -= stretch_coeff* 8.0* abs(angle)/PI + 5.0*(1.0 - stretch_coeff) ;
	float rays = decay(d,decay_f1,decay_f2,3.5);
	vec3 color = spark_color.rgb * rays ;
	color *= visible;
    COLOR = vec4(color, 1.0);
}
<?php
defined('_JEXEC') or die;

require_once __DIR__ . '/helper.php';

$app = JFactory::getApplication();
$input = $app->input;

if ($input->get('action') === 'send_contact') {
    $result = ModContactToggleHelper::sendMessage($params);
    echo json_encode($result);
    $app->close();
}

$buttonColor = $params->get('button_color', '#ff6b35');

require JModuleHelper::getLayoutPath('mod_contact_toggle');